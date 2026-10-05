const { app, BrowserWindow, ipcMain, globalShortcut } = require('electron');
app.disableHardwareAcceleration();
const path = require('path');
const { spawn } = require('child_process');

let settingsWindow;
let isVisible = true;
let currentConfig = { shape: 'Dot', size: 25, thick: 2, color: '#00ff66' };
let csharpProcess = null;

function sendToCSharp() {
    if (csharpProcess && !csharpProcess.killed) {
        let vis = isVisible ? "1" : "0";
        csharpProcess.stdin.write(`${currentConfig.shape}|${currentConfig.size}|${currentConfig.thick}|${currentConfig.color}|${vis}\n`);
    }
}

app.whenReady().then(() => {
    let crossExe = app.isPackaged ? path.join(process.resourcesPath, 'Crosshair.exe') : path.join(__dirname, 'Crosshair.exe');
    csharpProcess = spawn(crossExe);

    settingsWindow = new BrowserWindow({
        width: 700,
        height: 580,
        frame: false,
        transparent: true,
        resizable: false,
        icon: path.join(__dirname, 'icon.ico'),
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false
        }
    });
    settingsWindow.loadFile('index.html');

    ipcMain.on('update-crosshair', (event, data) => {
        currentConfig = data;
        sendToCSharp();
    });

    ipcMain.on('close-app', () => {
        app.quit();
    });

    ipcMain.on('minimize-app', () => {
        if (settingsWindow) settingsWindow.minimize();
    });

    ipcMain.on('update-hotkey', (event, key) => {
        globalShortcut.unregisterAll();
        if (key) {
            try {
                globalShortcut.register(key, () => {
                    isVisible = !isVisible;
                    sendToCSharp();
                });
            } catch (e) {}
        }
    });

    try {
        globalShortcut.register('Insert', () => {
            isVisible = !isVisible;
            sendToCSharp();
        });
    } catch(e){}
});

app.on('will-quit', () => {
    if (csharpProcess) csharpProcess.kill();
    globalShortcut.unregisterAll();
});
app.on('window-all-closed', () => {
    app.quit();
});
