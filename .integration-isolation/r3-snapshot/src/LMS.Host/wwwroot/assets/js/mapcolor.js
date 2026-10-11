window.colorSaudiMap = (regionData) => {
    const colorScale = value => {
        return value > 90 ? '#1A3856' :
            value > 50 ? '#1E405F' :
                value > 30 ? '#2C5D85' :
                    value > 10 ? '#4EA1E2' :
                            '#4EA1E2';
    };

    Object.entries(regionData).forEach(([regionId, value]) => {
        const region = document.getElementById(regionId);
        if (region) {
            region.style.fill = colorScale(value);
            region.style.stroke = "#fff";
            region.style.strokeWidth = "1";
            region.setAttribute("title", `${regionId}: ${value}`);
        }
    });
};
