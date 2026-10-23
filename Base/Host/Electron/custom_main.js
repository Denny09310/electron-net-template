import { ipcMain as ipc, BrowserWindow } from "electron";

const THEME = {
    light: {
        color: "#ffffff",
        symbolColor: "#333333"
    },
    dark: {
        color: "#171717",
        symbolColor: "#e5e5e5"
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