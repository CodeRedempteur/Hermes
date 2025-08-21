// wwwroot/js/secondary-navbar.js
window.initializeSecondaryNavbar = (dotNetHelper) => {
    let isSecondaryNavVisible = false;
    const headerHeight = 70; // Hauteur de votre header principal


    // Options pour l'Intersection Observer
    const observerOptions = {
        root: null,
        rootMargin: `-${headerHeight}px 0px 0px 0px`, // Décalage pour le header fixe
        threshold: [0, 0.1, 0.5] // Plusieurs seuils pour une détection plus précise
    };

    // Créer l'observateur pour la section produits
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            console.log(`👀 Observer: ${entry.target.id}`, {
                isIntersecting: entry.isIntersecting,
                intersectionRatio: entry.intersectionRatio.toFixed(2),
                boundingTop: entry.boundingClientRect.top.toFixed(0),
                boundingBottom: entry.boundingClientRect.bottom.toFixed(0)
            });

            if (entry.target.id === 'products') {
                if (entry.isIntersecting && entry.intersectionRatio > 0.1) {
                    // On entre dans la section produits
                    if (!isSecondaryNavVisible) {
                        console.log('✅ Affichage SecondaryNavbar');
                        isSecondaryNavVisible = true;
                        dotNetHelper.invokeMethodAsync('ShowSecondaryNavbar');
                    }
                } else if (entry.intersectionRatio <= 0) {
                    // On sort complètement de la section produits
                    if (isSecondaryNavVisible) {
                        console.log('❌ Masquage SecondaryNavbar');
                        isSecondaryNavVisible = false;
                        dotNetHelper.invokeMethodAsync('HideSecondaryNavbar');
                    }
                }
            }
        });
    }, observerOptions);

    // Attendre que le DOM soit chargé avant d'observer
    const initObserver = () => {
        // Observer la section produits
        const productsSection = document.getElementById('products');
        if (productsSection) {
            console.log('🎯 Section produits trouvée, observation démarrée');
            observer.observe(productsSection);
        } else {
            console.error('❌ Section #products non trouvée dans le DOM');
            // Essayer de nouveau après un délai
            setTimeout(() => {
                const retryProductsSection = document.getElementById('products');
                if (retryProductsSection) {
                    console.log('🎯 Section produits trouvée au second essai');
                    observer.observe(retryProductsSection);
                } else {
                    console.error('❌ Section #products toujours introuvable après retry');
                }
            }, 500);
        }

        // Observer aussi le topPage pour une détection plus précise
        const topPage = document.getElementById('topPage');
        if (topPage) {
            console.log('🔝 TopPage trouvé, observation démarrée');
            observer.observe(topPage);
        }
    };

    // Si le DOM est déjà chargé
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initObserver);
    } else {
        // Délai pour s'assurer que Blazor a rendu les composants
        setTimeout(initObserver, 200);
    }

    // Fonction de nettoyage pour éviter les fuites mémoire
    window.cleanupSecondaryNavbar = () => {
        console.log('🧹 Nettoyage SecondaryNavbar');
        observer.disconnect();
    };
};

// Alternative avec scroll listener si l'Intersection Observer ne convient pas
window.initializeSecondaryNavbarWithScroll = (dotNetHelper) => {
    let isSecondaryNavVisible = false;
    let ticking = false;

    console.log('🚀 Initialisation SecondaryNavbar avec scroll listener...');

    const checkProductsSection = () => {
        const productsSection = document.getElementById('products');
        if (!productsSection) {
            console.error('❌ Section #products non trouvée pour scroll listener');
            return;
        }

        const headerHeight = 70;
        const rect = productsSection.getBoundingClientRect();

        // La section est visible si son top est sous le header et son bottom est au-dessus du bas de l'écran
        const isInView = rect.top <= headerHeight && rect.bottom >= headerHeight;

        console.log('📏 Scroll check:', {
            rectTop: rect.top.toFixed(0),
            rectBottom: rect.bottom.toFixed(0),
            headerHeight: headerHeight,
            isInView: isInView,
            isSecondaryNavVisible: isSecondaryNavVisible
        });

        if (isInView && !isSecondaryNavVisible) {
            console.log('✅ Affichage SecondaryNavbar (scroll)');
            isSecondaryNavVisible = true;
            dotNetHelper.invokeMethodAsync('ShowSecondaryNavbar');
        } else if (!isInView && isSecondaryNavVisible) {
            console.log('❌ Masquage SecondaryNavbar (scroll)');
            isSecondaryNavVisible = false;
            dotNetHelper.invokeMethodAsync('HideSecondaryNavbar');
        }

        ticking = false;
    };

    const onScroll = () => {
        if (!ticking) {
            requestAnimationFrame(checkProductsSection);
            ticking = true;
        }
    };

    // Ajouter le listener de scroll
    window.addEventListener('scroll', onScroll, { passive: true });

    // Vérifier au chargement de la page
    setTimeout(checkProductsSection, 200);

    // Fonction de nettoyage
    window.cleanupSecondaryNavbar = () => {
        console.log('🧹 Nettoyage SecondaryNavbar (scroll)');
        window.removeEventListener('scroll', onScroll);
    };
};