const { ipcRenderer } = require("electron");

const root = document.documentElement;
let _theme = null;

function updateTheme() {
    // Read the theme actually applied by theme-init.js / Blazor.
    const theme = root.classList.contains("dark") ? "dark" : "light";

    // Avoid redundant IPC calls.
    if (theme === _theme) return;
    _theme = theme;

    ipcRenderer.invoke("set-titlebar-overlay", theme);
}

// Synchronize the title bar with the initial page theme.
updateTheme();

// Keep the title bar synchronized when Blazor changes the theme.
const observer = new MutationObserver(updateTheme);
observer.observe(root, {
    attributes: true,
    attributeFilter: ["class"]
});

// Also synchronize after Blazor enhanced navigation.
window.addEventListener("DOMContentLoaded", () => {
    Blazor.addEventListener("enhancedload", updateTheme);
});