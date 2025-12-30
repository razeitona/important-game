// Timezone Management System
// Handles automatic timezone detection, user selection, and date/time conversion

(function () {
    'use strict';

    // Timezone configuration
    const TIMEZONES = {
        'UTC': { offset: 0, label: 'UTC' },
        'GMT': { offset: 0, label: 'GMT' },
        'CET': { offset: 1, label: 'CET (Central European Time)' },
        'PST': { offset: -8, label: 'PST (Pacific Standard Time)' }
    };

    const STORAGE_KEY = 'user_timezone';
    const NOTIFICATION_DURATION = 4000; // 4 seconds

    // Timezone Manager Class
    class TimezoneManager {
        constructor() {
            this.currentTimezone = this.loadTimezone();
            this.init();
        }

        init() {
            // Detect browser timezone on first visit
            if (!localStorage.getItem(STORAGE_KEY)) {
                this.detectAndSetTimezone();
            }

            // Convert all dates on page load
            this.convertAllDates();

            // Setup timezone selector
            this.setupTimezoneSelector();
        }

        detectAndSetTimezone() {
            try {
                const browserTz = Intl.DateTimeFormat().resolvedOptions().timeZone;

                // Map browser timezone to our supported timezones
                let detectedTz = 'UTC';

                if (browserTz.includes('Europe/')) {
                    detectedTz = 'CET';
                } else if (browserTz.includes('America/Los_Angeles') || browserTz.includes('America/Vancouver')) {
                    detectedTz = 'PST';
                } else if (browserTz.includes('GMT') || browserTz.includes('London')) {
                    detectedTz = 'GMT';
                }

                this.setTimezone(detectedTz, true);
                this.showNotification(`Timezone automatically set to ${TIMEZONES[detectedTz].label}`);
            } catch (error) {
                console.error('Error detecting timezone:', error);
                this.setTimezone('UTC', false);
            }
        }

        loadTimezone() {
            return localStorage.getItem(STORAGE_KEY) || 'UTC';
        }

        setTimezone(timezone, save = true) {
            if (!TIMEZONES[timezone]) {
                console.error('Invalid timezone:', timezone);
                return;
            }

            this.currentTimezone = timezone;

            if (save) {
                localStorage.setItem(STORAGE_KEY, timezone);
            }

            // Update selector if it exists
            const selector = document.getElementById('timezoneSelector');
            if (selector) {
                selector.value = timezone;
            }
        }

        getTimezoneOffset() {
            return TIMEZONES[this.currentTimezone].offset;
        }

        convertUTCtoTimezone(utcDateString) {
            try {
                // Parse UTC date string (format: "dd/MM/yyyy HH:mm" or "dd/MM/yyyy")
                const parts = utcDateString.trim().split(' ');
                const dateParts = parts[0].split('/');

                if (dateParts.length !== 3) return utcDateString;

                let hours = 0;
                let minutes = 0;
                let hasTime = false;

                // Check if time is provided
                if (parts.length >= 2) {
                    const timeParts = parts[1].split(':');
                    if (timeParts.length === 2) {
                        hours = parseInt(timeParts[0]);
                        minutes = parseInt(timeParts[1]);
                        hasTime = true;
                    }
                }

                // Create UTC date
                const utcDate = new Date(Date.UTC(
                    parseInt(dateParts[2]), // year
                    parseInt(dateParts[1]) - 1, // month (0-indexed)
                    parseInt(dateParts[0]), // day
                    hours,
                    minutes
                ));

                // Add timezone offset
                const offsetHours = this.getTimezoneOffset();
                utcDate.setHours(utcDate.getHours() + offsetHours);

                // Format back
                const day = String(utcDate.getDate()).padStart(2, '0');
                const month = String(utcDate.getMonth() + 1).padStart(2, '0');
                const year = utcDate.getFullYear();

                if (hasTime) {
                    const formattedHours = String(utcDate.getHours()).padStart(2, '0');
                    const formattedMinutes = String(utcDate.getMinutes()).padStart(2, '0');
                    return `${day}/${month}/${year} ${formattedHours}:${formattedMinutes} (${this.currentTimezone})`;
                } else {
                    // Date only, no timezone label needed for date headers
                    return `${day}/${month}/${year}`;
                }
            } catch (error) {
                console.error('Error converting date:', error, utcDateString);
                return utcDateString;
            }
        }

        convertAllDates() {
            // Convert all time elements with data-utc-time attribute
            const timeElements = document.querySelectorAll('time[data-utc-time]');

            timeElements.forEach(element => {
                const utcTime = element.getAttribute('data-utc-time');
                const convertedTime = this.convertUTCtoTimezone(utcTime);
                element.textContent = convertedTime;
                element.setAttribute('title', `UTC: ${utcTime}`);
            });
        }

        setupTimezoneSelector() {
            const selector = document.getElementById('timezoneSelector');
            if (!selector) return;

            // Set current value
            selector.value = this.currentTimezone;

            // Listen for changes
            selector.addEventListener('change', (e) => {
                const newTimezone = e.target.value;
                this.setTimezone(newTimezone, true);
                this.convertAllDates();
                this.showNotification(`Timezone changed to ${TIMEZONES[newTimezone].label}`);
            });
        }

        showNotification(message) {
            // Remove any existing notification
            const existingNotification = document.getElementById('timezoneNotification');
            if (existingNotification) {
                existingNotification.remove();
            }

            // Create notification element
            const notification = document.createElement('div');
            notification.id = 'timezoneNotification';
            notification.className = 'timezone-notification';
            notification.textContent = message;

            // Add to body
            document.body.appendChild(notification);

            // Show notification with animation
            setTimeout(() => {
                notification.classList.add('show');
            }, 100);

            // Hide and remove after duration
            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => {
                    notification.remove();
                }, 300);
            }, NOTIFICATION_DURATION);
        }
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            window.timezoneManager = new TimezoneManager();
        });
    } else {
        window.timezoneManager = new TimezoneManager();
    }
})();

// Loading State Manager
// Handles skeleton loading states for better UX during content updates
(function () {
    'use strict';

    class LoadingStateManager {
        /**
         * Show skeleton loading state for a section
         * @param {string} sectionId - The ID of the section container
         * @param {number} duration - Minimum duration to show skeleton (ms)
         */
        showLoading(sectionId, duration = 300) {
            const section = document.getElementById(sectionId);
            if (!section) {
                console.warn(`Section ${sectionId} not found`);
                return;
            }

            const skeletonContent = section.querySelector('.skeleton-content');
            const actualContent = section.querySelector('.actual-content');

            if (!skeletonContent || !actualContent) {
                console.warn(`Skeleton or actual content not found in ${sectionId}`);
                return;
            }

            // Hide actual content, show skeleton
            actualContent.style.display = 'none';
            skeletonContent.style.display = 'grid';

            // Store minimum display time
            this._loadingStartTime = Date.now();
            this._minDuration = duration;
        }

        /**
         * Hide skeleton loading state and show actual content
         * @param {string} sectionId - The ID of the section container
         */
        async hideLoading(sectionId) {
            const section = document.getElementById(sectionId);
            if (!section) {
                console.warn(`Section ${sectionId} not found`);
                return;
            }

            const skeletonContent = section.querySelector('.skeleton-content');
            const actualContent = section.querySelector('.actual-content');

            if (!skeletonContent || !actualContent) {
                console.warn(`Skeleton or actual content not found in ${sectionId}`);
                return;
            }

            // Ensure minimum display time for smooth UX
            if (this._loadingStartTime && this._minDuration) {
                const elapsed = Date.now() - this._loadingStartTime;
                const remaining = Math.max(0, this._minDuration - elapsed);

                if (remaining > 0) {
                    await new Promise(resolve => setTimeout(resolve, remaining));
                }
            }

            // Show actual content, hide skeleton
            skeletonContent.style.display = 'none';
            actualContent.style.display = '';

            // Reset timing
            this._loadingStartTime = null;
            this._minDuration = null;
        }

        /**
         * Simulate loading state for demonstration/testing
         * @param {string} sectionId - The ID of the section container
         * @param {number} duration - How long to show skeleton (ms)
         */
        async simulateLoading(sectionId, duration = 2000) {
            this.showLoading(sectionId, 300);
            await new Promise(resolve => setTimeout(resolve, duration));
            await this.hideLoading(sectionId);
        }

        /**
         * Wrap a function with loading state
         * @param {string} sectionId - The ID of the section container
         * @param {Function} asyncFunction - The async function to execute
         * @param {number} minDuration - Minimum duration to show loading (ms)
         */
        async withLoading(sectionId, asyncFunction, minDuration = 300) {
            this.showLoading(sectionId, minDuration);
            try {
                const result = await asyncFunction();
                await this.hideLoading(sectionId);
                return result;
            } catch (error) {
                await this.hideLoading(sectionId);
                throw error;
            }
        }

        /**
         * Show a loading spinner overlay
         * @param {string} message - Loading message to display
         */
        showSpinner(message = 'Loading...') {
            // Remove existing spinner if any
            this.hideSpinner();

            const spinner = document.createElement('div');
            spinner.id = 'globalLoadingSpinner';
            spinner.className = 'loading-spinner';
            spinner.innerHTML = `
                <div class="spinner"></div>
                <div class="spinner-text">${message}</div>
            `;

            document.body.appendChild(spinner);
        }

        /**
         * Hide the loading spinner overlay
         */
        hideSpinner() {
            const spinner = document.getElementById('globalLoadingSpinner');
            if (spinner) {
                spinner.remove();
            }
        }
    }

    // Initialize and expose globally
    window.loadingManager = new LoadingStateManager();

    // Example usage for demonstration (can be removed in production)
    // Uncomment to test skeleton loading on page load:
    //
    // document.addEventListener('DOMContentLoaded', () => {
    //     // Simulate loading for homepage sections
    //     if (document.getElementById('trending-matches-section')) {
    //         loadingManager.simulateLoading('trending-matches-section', 1500);
    //     }
    //     if (document.getElementById('upcoming-matches-section')) {
    //         loadingManager.simulateLoading('upcoming-matches-section', 1800);
    //     }
    // });

    // Sticky Navigation Scroll Detection
    (function initStickyNavigation() {
        const header = document.querySelector('header');
        if (!header) return;

        let lastScrollY = window.scrollY;
        let ticking = false;

        function updateHeader() {
            const scrollY = window.scrollY;

            if (scrollY > 10) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }

            lastScrollY = scrollY;
            ticking = false;
        }

        function onScroll() {
            if (!ticking) {
                window.requestAnimationFrame(updateHeader);
                ticking = true;
            }
        }

        window.addEventListener('scroll', onScroll, { passive: true });
    })();

    // Share Functionality
    (function initShareButtons() {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = 'share-notification';
        document.body.appendChild(notification);

        function showNotification(message) {
            notification.innerHTML = `<i class="bi bi-check-circle"></i> ${message}`;
            notification.classList.add('show');

            setTimeout(() => {
                notification.classList.remove('show');
            }, 3000);
        }

        function shareWhatsApp(title, score, url) {
            const text = `🔥 ${title} - Excitement Score: ${score}%\nCheck out this match on Match to Watch!`;
            const fullUrl = window.location.origin + url;
            const whatsappUrl = `https://wa.me/?text=${encodeURIComponent(text + '\n' + fullUrl)}`;
            window.open(whatsappUrl, '_blank');
        }

        function shareTwitter(title, score, url) {
            const text = `🔥 ${title} - Excitement Score: ${score}%`;
            const fullUrl = window.location.origin + url;
            const hashtags = 'MatchToWatch,Football,Soccer';
            const twitterUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(fullUrl)}&hashtags=${hashtags}`;
            window.open(twitterUrl, '_blank');
        }

        function shareFacebook(url) {
            const fullUrl = window.location.origin + url;
            const facebookUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(fullUrl)}`;
            window.open(facebookUrl, '_blank');
        }

        function copyLink(url) {
            const fullUrl = window.location.origin + url;
            navigator.clipboard.writeText(fullUrl).then(() => {
                showNotification('Link copied to clipboard!');
            }).catch(() => {
                // Fallback for older browsers
                const textArea = document.createElement('textarea');
                textArea.value = fullUrl;
                textArea.style.position = 'fixed';
                textArea.style.left = '-999999px';
                document.body.appendChild(textArea);
                textArea.select();
                try {
                    document.execCommand('copy');
                    showNotification('Link copied to clipboard!');
                } catch (err) {
                    showNotification('Failed to copy link');
                }
                document.body.removeChild(textArea);
            });
        }

        // Handle share buttons (Match Detail Page)
        document.addEventListener('click', function(e) {
            const shareBtn = e.target.closest('.share-btn');
            if (!shareBtn) return;

            e.preventDefault();
            e.stopPropagation();

            const shareType = shareBtn.dataset.shareType;
            const matchTitle = shareBtn.dataset.matchTitle;
            const matchScore = shareBtn.dataset.matchScore;

            // Get URL from current page or from match card data
            let matchUrl = window.location.pathname;
            const matchCard = shareBtn.closest('.match-card-simple');
            if (matchCard) {
                const trigger = matchCard.querySelector('.match-card-share-trigger');
                if (trigger) {
                    matchUrl = trigger.dataset.matchUrl;
                }
            }

            switch(shareType) {
                case 'whatsapp':
                    shareWhatsApp(matchTitle, matchScore, matchUrl);
                    break;
                case 'twitter':
                    shareTwitter(matchTitle, matchScore, matchUrl);
                    break;
                case 'facebook':
                    shareFacebook(matchUrl);
                    break;
                case 'copy':
                    copyLink(matchUrl);
                    break;
            }
        });

        // Handle match card share trigger (open overlay)
        document.addEventListener('click', function(e) {
            const trigger = e.target.closest('.match-card-share-trigger');
            if (!trigger) return;

            e.preventDefault();
            e.stopPropagation();

            const matchCard = trigger.closest('.match-card-simple');
            if (!matchCard) return;

            const overlay = matchCard.querySelector('.share-overlay');
            if (overlay) {
                overlay.classList.add('active');
            }
        });

        // Handle overlay close button
        document.addEventListener('click', function(e) {
            const closeBtn = e.target.closest('.share-overlay-close');
            if (!closeBtn) return;

            e.preventDefault();
            e.stopPropagation();

            const overlay = closeBtn.closest('.share-overlay');
            if (overlay) {
                overlay.classList.remove('active');
            }
        });

        // Close overlay when clicking outside
        document.addEventListener('click', function(e) {
            const overlay = e.target.closest('.share-overlay');
            if (overlay && e.target === overlay) {
                overlay.classList.remove('active');
            }
        });

        // Prevent overlay clicks from closing when clicking inside
        document.addEventListener('click', function(e) {
            if (e.target.closest('.share-overlay-buttons') || e.target.closest('.share-overlay-title')) {
                e.stopPropagation();
            }
        });
    })();

})();
