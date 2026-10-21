const { ipcRenderer } = require("electron");

const root = document.documentElement;
let _theme = null;

function updateTheme() {
    const theme = root.classList.contains("dark") ? "dark" : "light";

    if (theme === _theme) return;
    _theme = theme;

    ipcRenderer.invoke("set-titlebar-overlay", theme);
}

updateTheme();

const observer = new MutationObserver(updateTheme);
observer.observe(root, {
    attributes: true,
    attributeFilter: ["class"]
});

window.addEventListener("DOMContentLoaded", () => {
    Blazor.addEventListener("enhancedload", updateTheme);
});