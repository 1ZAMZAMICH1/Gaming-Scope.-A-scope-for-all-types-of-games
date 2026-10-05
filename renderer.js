const { ipcRenderer } = require('electron');

let currentShape = 'Dot';
window.setShape = (s) => {
    currentShape = s;
    document.querySelectorAll('.switch-group button').forEach(b => b.classList.remove('active'));
    event.target.classList.add('active');
    update();
}

document.getElementById('color').oninput = update;
document.getElementById('size').oninput = update;
document.getElementById('thick').oninput = update;

function update() {
    ipcRenderer.send('update-crosshair', {
        color: document.getElementById('color').value,
        size: parseInt(document.getElementById('size').value),
        thick: parseInt(document.getElementById('thick').value),
        shape: currentShape
    });
}

// Initial draw signal
setTimeout(update, 500);

let isBinding = false;
window.startBind = () => {
    isBinding = true;
    let b = document.getElementById('btn-bind');
    b.innerText = "... PRESS ANY KEY ...";
    b.classList.add('binding');
}

window.addEventListener('keydown', (e) => {
    if (isBinding) {
        let key = e.key;
        if(key.length === 1) key = key.toUpperCase();
        
        // Handling special cases
        if (key === 'Control') key = 'Ctrl';
        if (key === 'Escape') key = 'Esc';
        
        isBinding = false;
        let b = document.getElementById('btn-bind');
        b.innerText = key;
        b.classList.remove('binding');
        
        ipcRenderer.send('update-hotkey', key);
    }
});
