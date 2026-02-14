export default function register() {
    const tabs = [...document.querySelectorAll(".sw.tab-container > header input[type='radio'][data-register]")];

    for(const tab of tabs) {
        tab.addEventListener("change", e => activateTab(e.target));
    }
}

function activateTab(tabHeader) {
    const tabPanel = tabHeader.closest(".sw.tab-container").querySelector("& > [role='tabpanel']");

    tabPanel.querySelectorAll("& > div").forEach(hide);

    const activePanel = tabPanel.querySelector(`& > div[data-identifier='${tabHeader.value}']`);
    show(activePanel);
}

function show(panel) {
    panel.removeAttribute("hidden");
    panel.removeAttribute("aria-hidden");
}

function hide(panel) {
    panel.setAttribute("hidden", "");
    panel.setAttribute("aria-hidden", "true");
}
