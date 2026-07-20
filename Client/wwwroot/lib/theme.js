const { ipcRenderer } = require("electron");

const media = window.matchMedia("(prefers-color-scheme: dark)");

function updateTheme(isDark) {
    document.documentElement.classList.toggle("dark", isDark);
    ipcRenderer.invoke("set-titlebar-overlay", isDark ? "dark" : "light");
}

document.addEventListener('DOMContentLoaded', (e) => {

    // update on initial page load
    updateTheme(media.matches);

    // then update on partial page reloads
    Blazor.addEventListener('enhancedload', () => {
        updateTheme(media.matches);
    });

    media.addEventListener("change", (event) => {
        updateTheme(event.matches);
    });
})