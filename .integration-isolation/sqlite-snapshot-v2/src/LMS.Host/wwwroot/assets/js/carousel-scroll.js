window.scrollCarousel = (carouselElement, direction) => {
    if (carouselElement) {
        const offset = 350; // adjust scroll amount
        carouselElement.scrollBy({ left: direction * offset * -1, behavior: 'smooth' });
    }
};
