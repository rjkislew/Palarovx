window.scrollContainer = function (container, x, y) {
    if (container) {
        container.scrollBy(x, y);
    }
};

window.scrollContainer.scrollLeft = function (container) {
    if (container) {
        return container.scrollLeft;
    }
    return 0;
};

window.scrollContainer.scrollWidth = function (container) {
    if (container) {
        return container.scrollWidth;
    }
    return 0;
};

window.scrollContainer.clientWidth = function (container) {
    if (container) {
        return container.clientWidth;
    }
    return 0;
};
