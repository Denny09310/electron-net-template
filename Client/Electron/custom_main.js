import { ipcMain as ipc, BrowserWindow } from "electron";

const THEME = {
    light: {
        color: "#ffffff",
        symbolColor: "#000000"
    },
    dark: {
        color: "#0a0a0a",
        symbolColor: "#ffffff"
    }
}

export function onStartup(host) {

    ipc.handle("set-titlebar-overlay", (event, name) => {
        const theme = THEME[name];
        if (!theme) return;

        const win = BrowserWindow.fromWebContents(event.sender);
        if (!win) return;

        win.setTitleBarOverlay({
            color: theme.color,
            symbolColor: theme.symbolColor,
            height: 32
        });
    });

    return true;
}