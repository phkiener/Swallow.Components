// Mostly taken from https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Reference/Roles/tab_role

export default function registerTabs(element: HTMLElement) {
    const candidates = element.querySelectorAll<HTMLDivElement>(".sw-tabs:not([data-registered])");
    for (const candidate of candidates) {
        registerTab(candidate);
    }
}

function registerTab(element: HTMLDivElement) {
    const tabList = element.querySelector<HTMLDivElement>(":scope > .tab-list");
    tabList?.addEventListener("keydown", handleTabListInput);

    const { Tabs: tabs } = inspect(element);
    for (const tab of tabs) {

        tab.addEventListener("click", handleTabClick);
        tab.addEventListener("keydown", handleTabInput);
    }

    element.setAttribute("data-registered", "true");
}

function handleTabClick(event: MouseEvent) {
    activate(event.target as HTMLButtonElement);
}

function handleTabInput(event: KeyboardEvent) {
    if (event.key === "Enter" || event.key === " ") {
        event.preventDefault();
        event.stopPropagation();

        activate(event.target as HTMLButtonElement);
    }
}

function handleTabListInput(event: KeyboardEvent) {
    const { Tabs: tabs } = inspect(event.target as HTMLButtonElement);
    const currentIndex = tabs.indexOf(event.target as HTMLButtonElement);

    if (currentIndex === -1) {
        return;
    }

    let targetIndex = 0;
    switch (event.key) {
        case "ArrowRight":
            targetIndex = (currentIndex + 1) % tabs.length;
            break;

        case "ArrowLeft":
            targetIndex = (currentIndex - 1) % tabs.length;
            break;

        case "Home":
            targetIndex = 0;
            break;

        case "End":
            targetIndex = tabs.length - 1;
            break;

        default:
            return;
    }

    event.preventDefault();
    event.stopPropagation();

    tabs[targetIndex]?.focus();

}

function activate(targetTab: HTMLButtonElement) {
    const targetPanelId = targetTab.getAttribute("aria-controls");
    const targetPanel = targetPanelId ? document.getElementById(targetPanelId) : null;

    const container = inspect(targetTab);

    for (const tab of container.Tabs) {
        if (tab === targetTab) {
            continue;
        }

        tab.setAttribute("aria-selected", "false");
        tab.setAttribute("tabindex", "-1");
    }

    for (const panel of container.Panels) {
        if (panel === targetPanel) {
            continue;
        }

        panel.hidden = true;
    }

    targetTab.setAttribute("aria-selected", "true");
    targetTab.setAttribute("tabindex", "0");
    if (targetPanel) {
        targetPanel.hidden = false;
    }

}

function inspect(element: HTMLElement | null): TabInfo {
    const container = element?.closest(".sw-tabs");
    if (!container) {
        return { Tabs: [], Panels: [] };
    }

    const tabs = Array.from(container.querySelectorAll<HTMLButtonElement>(':scope > .tab-list > [role="tab"]'));
    const panels = Array.from(container.querySelectorAll<HTMLDivElement>(':scope > .tab-panels > [role="tabpanel"]'));

    return { Tabs: tabs, Panels: panels };
}

type TabInfo = { Tabs: HTMLButtonElement[], Panels: HTMLDivElement[] };
