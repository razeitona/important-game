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
                    return `${day}/${month}/${year} ${formattedHours}:${formattedMinutes}`;
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
            const text = `${title} - Excitement Score: ${score}%\nCheck out this match on Match to Watch!`;
            const fullUrl = window.location.origin + url;
            const whatsappUrl = `https://wa.me/?text=${encodeURIComponent(text + '\n' + fullUrl)}`;
            window.open(whatsappUrl, '_blank');
        }

        function shareTwitter(title, score, url) {
            const text = `${title} - Excitement Score: ${score}%`;
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

    // Swipe Gestures for Match Detail Page
    (function initSwipeGestures() {
        // Only run on match detail page
        const matchDetail = document.querySelector('.match-detail');
        if (!matchDetail) return;

        let startX = 0;
        let startY = 0;
        let startTime = 0;
        const minSwipeDistance = 50;  // Minimum distance for swipe (pixels)
        const maxSwipeTime = 300;     // Maximum time for swipe (milliseconds)
        const maxVerticalDistance = 100; // Max vertical movement to still count as horizontal swipe

        function handleTouchStart(e) {
            const touch = e.touches[0];
            startX = touch.clientX;
            startY = touch.clientY;
            startTime = Date.now();
        }

        function handleTouchEnd(e) {
            const touch = e.changedTouches[0];
            const endX = touch.clientX;
            const endY = touch.clientY;
            const endTime = Date.now();

            const diffX = startX - endX;
            const diffY = Math.abs(startY - endY);
            const diffTime = endTime - startTime;

            // Check if it's a valid horizontal swipe
            if (Math.abs(diffX) > minSwipeDistance &&
                diffY < maxVerticalDistance &&
                diffTime < maxSwipeTime) {

                if (diffX > 0) {
                    // Swiped left - go to next match
                    navigateToNextMatch();
                } else {
                    // Swiped right - go to previous match
                    navigateToPreviousMatch();
                }
            }
        }

        function navigateToNextMatch() {
            // Get next match URL from session storage or navigate back to matches
            const nextMatchUrl = sessionStorage.getItem('nextMatchUrl');
            if (nextMatchUrl) {
                window.location.href = nextMatchUrl;
            } else {
                // Fallback: show notification
                if (window.timezoneManager) {
                    window.timezoneManager.showNotification('No next match available');
                }
            }
        }

        function navigateToPreviousMatch() {
            // Get previous match URL from session storage or navigate back
            const prevMatchUrl = sessionStorage.getItem('prevMatchUrl');
            if (prevMatchUrl) {
                window.location.href = prevMatchUrl;
            } else {
                // Fallback: go back to matches page
                window.location.href = '/matches';
            }
        }

        // Add touch event listeners
        matchDetail.addEventListener('touchstart', handleTouchStart, { passive: true });
        matchDetail.addEventListener('touchend', handleTouchEnd, { passive: true });

        // Keyboard shortcuts for desktop
        document.addEventListener('keydown', function(e) {
            // Left arrow key = previous match
            if (e.key === 'ArrowLeft' && !e.ctrlKey && !e.altKey && !e.shiftKey) {
                const activeElement = document.activeElement;
                // Don't trigger if user is typing in an input
                if (activeElement.tagName !== 'INPUT' && activeElement.tagName !== 'TEXTAREA') {
                    e.preventDefault();
                    navigateToPreviousMatch();
                }
            }
            // Right arrow key = next match
            else if (e.key === 'ArrowRight' && !e.ctrlKey && !e.altKey && !e.shiftKey) {
                const activeElement = document.activeElement;
                if (activeElement.tagName !== 'INPUT' && activeElement.tagName !== 'TEXTAREA') {
                    e.preventDefault();
                    navigateToNextMatch();
                }
            }
        });

        // Visual feedback for swipe
        let swipeIndicator = null;
        function showSwipeIndicator(direction) {
            if (!swipeIndicator) {
                swipeIndicator = document.createElement('div');
                swipeIndicator.style.cssText = `
                    position: fixed;
                    top: 50%;
                    ${direction === 'left' ? 'right: 20px;' : 'left: 20px;'}
                    transform: translateY(-50%);
                    background: var(--color-primary, #258cfb);
                    color: white;
                    padding: 1rem 1.5rem;
                    border-radius: var(--radius-md, 8px);
                    font-size: 1.5rem;
                    z-index: 9999;
                    pointer-events: none;
                    opacity: 0;
                    transition: opacity 0.2s ease;
                `;
                swipeIndicator.innerHTML = direction === 'left' ? '<i class="bi bi-arrow-right"></i>' : '<i class="bi bi-arrow-left"></i>';
                document.body.appendChild(swipeIndicator);
            }

            swipeIndicator.style.opacity = '1';
            setTimeout(() => {
                if (swipeIndicator) {
                    swipeIndicator.style.opacity = '0';
                }
            }, 300);
        }
    })();

    // Calendar View Functionality
    (function initCalendar() {
        // Only run on calendar page
        if (!document.querySelector('.calendar-container')) return;

        const modal = document.getElementById('calendarModal');
        const modalTitle = document.getElementById('modalTitle');
        const modalBody = document.getElementById('modalBody');
        const closeModalBtn = document.getElementById('closeModalBtn');
        const calendarDays = document.querySelectorAll('.calendar-day.has-matches');
        const prevMonthBtn = document.getElementById('prevMonthBtn');
        const nextMonthBtn = document.getElementById('nextMonthBtn');

        // Month navigation
        if (prevMonthBtn) {
            prevMonthBtn.addEventListener('click', function () {
                const year = parseInt(this.dataset.year);
                const month = parseInt(this.dataset.month);
                navigateToMonth(year, month, -1);
            });
        }

        if (nextMonthBtn) {
            nextMonthBtn.addEventListener('click', function () {
                const year = parseInt(this.dataset.year);
                const month = parseInt(this.dataset.month);
                navigateToMonth(year, month, 1);
            });
        }

        function navigateToMonth(year, month, direction) {
            let newMonth = month + direction;
            let newYear = year;

            if (newMonth < 1) {
                newMonth = 12;
                newYear--;
            } else if (newMonth > 12) {
                newMonth = 1;
                newYear++;
            }

            window.location.href = `/calendar?year=${newYear}&month=${newMonth}`;
        }

        // Swipe gestures for month navigation
        let startX = 0;
        let startY = 0;
        const calendarGrid = document.querySelector('.calendar-grid');

        if (calendarGrid) {
            calendarGrid.addEventListener('touchstart', function(e) {
                startX = e.touches[0].clientX;
                startY = e.touches[0].clientY;
            }, { passive: true });

            calendarGrid.addEventListener('touchend', function(e) {
                const endX = e.changedTouches[0].clientX;
                const endY = e.changedTouches[0].clientY;
                const diffX = startX - endX;
                const diffY = Math.abs(startY - endY);

                if (Math.abs(diffX) > 50 && diffY < 100) {
                    const year = parseInt(prevMonthBtn.dataset.year);
                    const month = parseInt(prevMonthBtn.dataset.month);

                    if (diffX > 0) {
                        navigateToMonth(year, month, 1); // Swipe left = next month
                    } else {
                        navigateToMonth(year, month, -1); // Swipe right = prev month
                    }
                }
            }, { passive: true });
        }

        // Event delegation for calendar day interactions
        document.addEventListener('click', function(e) {
            // Handle "Ver todos" link click
            const viewAllLink = e.target.closest('.calendar-view-all');
            if (viewAllLink) {
                e.stopPropagation();
                const dayNumber = parseInt(viewAllLink.dataset.day);
                const year = parseInt(viewAllLink.dataset.year);
                const month = parseInt(viewAllLink.dataset.month);
                showMatchesForDay(dayNumber, year, month);
                return;
            }

            // Handle mini match card click
            const miniMatchCard = e.target.closest('.mini-match-card');
            if (miniMatchCard) {
                e.stopPropagation();
                const matchId = miniMatchCard.dataset.matchId;
                if (matchId && window.calendarMatches) {
                    // Find the match in calendar data
                    for (const day in window.calendarMatches) {
                        const match = window.calendarMatches[day].find(m => m.matchId == matchId);
                        if (match) {
                            const matchUrl = `/match/${match.homeSlug}-vs-${match.awaySlug}`;
                            window.location.href = matchUrl;
                            return;
                        }
                    }
                }
                return;
            }

            // Handle calendar day click (only if not clicking on specific elements)
            const calendarDay = e.target.closest('.calendar-day.has-matches');
            if (calendarDay && !e.target.closest('.mini-match-card') && !e.target.closest('.calendar-view-all')) {
                const dayNumber = parseInt(calendarDay.dataset.day);
                const year = parseInt(calendarDay.dataset.year);
                const month = parseInt(calendarDay.dataset.month);
                showMatchesForDay(dayNumber, year, month);
            }
        });

        function showMatchesForDay(day, year, month) {
            if (!window.calendarMatches || !window.calendarMatches[day]) {
                return;
            }

            const matches = window.calendarMatches[day];
            const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
                'July', 'August', 'September', 'October', 'November', 'December'];

            modalTitle.textContent = `Matches on ${monthNames[month - 1]} ${day}, ${year}`;

            // Build matches HTML
            let matchesHTML = '<div class="calendar-modal-matches-grid">';

            matches.forEach(match => {
                const esScore = Math.round(match.excitmentScore * 100);
                const esDecimal = match.excitmentScore.toFixed(2).replace(',', '.');
                const matchUrl = `/match/${match.homeSlug}-vs-${match.awaySlug}`;

                matchesHTML += `
                    <div class="card match-card-simple"
                         style="--bgcolor: ${match.competitionBgColor};"
                         data-game-score="${esScore}"
                         data-game-league="${match.competitionId}">
                        <a href="${matchUrl}" class="match-card-link">
                            <div class="card-header match-header">
                                <div>
                                    <img src="/images/competition/${match.competitionId}.png" alt="${match.competitionName}" width="1.2rem" />
                                </div>
                                <div class="match-header-title">${match.competitionName}</div>
                                ${match.isLive ?
                                    '<div class="match-card-live"><span>LIVE</span></div>' :
                                    `<div class="match-card-time"><time>${match.matchDate}</time></div>`
                                }
                            </div>
                            <div class="card-body match-card-body">
                                <div class="match-card-content">
                                    <div class="match-card-team">
                                        <div class="match-card-team-logo">
                                            <img src="/images/team/${match.homeTeamId}.png" alt="${match.homeTeamName}" width="80px" />
                                        </div>
                                        <div class="match-card-team-name">
                                            <span>${match.homeTeamName}</span>
                                        </div>
                                    </div>
                                    <div class="match-card-score">
                                        <div class="match-card-score-value" data-es="${esDecimal}" title="Excitement Score">
                                            <i class="bi bi-fire es-fire-icon"></i> ${esScore}
                                        </div>
                                        <span>Excitment score</span>
                                    </div>
                                    <div class="match-card-team">
                                        <div class="match-card-team-logo">
                                            <img src="/images/team/${match.awayTeamId}.png" alt="${match.awayTeamName}" width="80px" />
                                        </div>
                                        <div class="match-card-team-name">
                                            <span>${match.awayTeamName}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </a>
                    </div>
                `;
            });

            matchesHTML += '</div>';
            modalBody.innerHTML = matchesHTML;

            // Show modal
            modal.classList.add('active');
        }


        // Close modal handlers
        if (closeModalBtn) {
            closeModalBtn.addEventListener('click', closeModal);
        }

        if (modal) {
            modal.querySelector('.calendar-modal-overlay').addEventListener('click', closeModal);
        }

        function closeModal() {
            modal.classList.remove('active');
        }

        // Keyboard shortcuts
        document.addEventListener('keydown', function (e) {
            const activeElement = document.activeElement;
            if (activeElement.tagName === 'INPUT' || activeElement.tagName === 'TEXTAREA') {
                return;
            }

            if (e.key === 'Escape' && modal.classList.contains('active')) {
                closeModal();
            } else if (e.key === 'ArrowLeft') {
                const year = parseInt(prevMonthBtn.dataset.year);
                const month = parseInt(prevMonthBtn.dataset.month);
                navigateToMonth(year, month, -1);
            } else if (e.key === 'ArrowRight') {
                const year = parseInt(prevMonthBtn.dataset.year);
                const month = parseInt(prevMonthBtn.dataset.month);
                navigateToMonth(year, month, 1);
            }
        });
    })();

    // Login Modal Handler
    (function initLoginModal() {
        const loginBtn = document.getElementById('loginBtn');
        const loginModal = document.getElementById('loginModal');
        const closeLoginModal = document.getElementById('closeLoginModal');
        const googleSignInBtn = document.getElementById('googleSignInBtn');
        const modalOverlay = loginModal?.querySelector('.login-modal-overlay');

        if (!loginBtn || !loginModal) return;

        // Open modal
        loginBtn.addEventListener('click', function () {
            loginModal.classList.add('active');
            document.body.style.overflow = 'hidden'; // Prevent scroll
        });

        // Close modal functions
        function closeModal() {
            loginModal.classList.remove('active');
            document.body.style.overflow = ''; // Re-enable scroll
        }

        if (closeLoginModal) {
            closeLoginModal.addEventListener('click', closeModal);
        }

        if (modalOverlay) {
            modalOverlay.addEventListener('click', closeModal);
        }

        // Close on Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && loginModal.classList.contains('active')) {
                closeModal();
            }
        });

        // Google Sign In
        if (googleSignInBtn) {
            googleSignInBtn.addEventListener('click', function () {
                // Get current page URL to return after login
                const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                window.location.href = `/login?returnUrl=${returnUrl}`;
            });
        }

        // Close modal if user is authenticated (after redirect from login)
        // Check if login button is not present (means user is authenticated)
        if (!loginBtn && loginModal.classList.contains('active')) {
            closeModal();
        }
    })();

    // Match Like/Unlike Handler
    (function initMatchLikes() {
        const likeButtons = document.querySelectorAll('.match-card-like-btn, .match-detail-like-btn');

        if (likeButtons.length === 0) return;

        // Initialize like states for all matches on page load
        async function initializeLikeStates() {
            for (const btn of likeButtons) {
                const matchId = parseInt(btn.dataset.matchId);
                if (!matchId) continue;

                try {
                    const response = await fetch(`/api/matchlike?matchId=${matchId}`);
                    if (response.ok) {
                        const data = await response.json();
                        updateLikeButton(btn, data.liked, data.voteCount);
                    }
                } catch (error) {
                    console.error('Error initializing like state:', error);
                }
            }
        }

        // Update button visual state
        function updateLikeButton(button, isLiked, voteCount) {
            button.dataset.liked = isLiked;
            const icon = button.querySelector('i');
            const countSpan = button.querySelector('.like-count');

            if (isLiked) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                button.classList.add('liked');
            } else {
                icon.classList.remove('bi-heart-fill');
                icon.classList.add('bi-heart');
                button.classList.remove('liked');
            }

            if (countSpan) {
                countSpan.textContent = voteCount || 0;
                countSpan.style.display = voteCount > 0 ? 'inline' : 'none';
            }
        }

        // Handle like/unlike toggle
        async function toggleLike(button) {
            const matchId = parseInt(button.dataset.matchId);
            const isCurrentlyLiked = button.dataset.liked === 'true';

            // Optimistic UI update
            const newState = !isCurrentlyLiked;
            button.disabled = true;

            try {
                const method = newState ? 'POST' : 'DELETE';
                const response = await fetch('/api/matchlike', {
                    method: method,
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify({ matchId: matchId })
                });

                if (response.ok) {
                    const data = await response.json();
                    updateLikeButton(button, data.liked, data.voteCount);

                    // Show feedback animation
                    button.classList.add('like-animate');
                    setTimeout(() => button.classList.remove('like-animate'), 300);
                } else if (response.status === 401) {
                    // User not authenticated - show login modal
                    const loginModal = document.getElementById('loginModal');
                    if (loginModal) {
                        loginModal.classList.add('active');
                        document.body.style.overflow = 'hidden';
                    }
                } else {
                    throw new Error('Failed to toggle like');
                }
            } catch (error) {
                console.error('Error toggling like:', error);
                // Show error notification if available
                if (window.timezoneManager) {
                    window.timezoneManager.showNotification('Failed to like match. Please try again.');
                }
            } finally {
                button.disabled = false;
            }
        }

        // Attach event listeners
        likeButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleLike(this);
            });
        });

        // Initialize states on page load
        initializeLikeStates();
    })();

    // =============================================================================
    // FAVORITE TEAMS SYSTEM
    // =============================================================================
    (function () {
        const favoriteButtons = document.querySelectorAll('.team-favorite-btn');
        if (favoriteButtons.length === 0) return;

        // Helper to update button state
        function updateFavoriteButton(button, isFavorite) {
            button.setAttribute('data-favorite', isFavorite);
            const icon = button.querySelector('i');
            if (isFavorite) {
                icon.classList.remove('bi-star');
                icon.classList.add('bi-star-fill');
            } else {
                icon.classList.remove('bi-star-fill');
                icon.classList.add('bi-star');
            }
        }

        // Initialize favorite states for all team buttons
        async function initializeFavoriteStates() {
            for (const button of favoriteButtons) {
                const teamId = parseInt(button.getAttribute('data-team-id'));
                if (!teamId) continue;

                try {
                    const response = await fetch(`/Api/FavoriteTeam?teamId=${teamId}`);
                    if (response.ok) {
                        const data = await response.json();
                        updateFavoriteButton(button, data.isFavorite);
                    }
                } catch (error) {
                    console.error('Error loading favorite state:', error);
                }
            }
        }

        // Toggle favorite state
        async function toggleFavorite(button) {
            const teamId = parseInt(button.getAttribute('data-team-id'));
            const isFavorite = button.getAttribute('data-favorite') === 'true';

            button.disabled = true;

            try {
                const method = isFavorite ? 'DELETE' : 'POST';
                const response = await fetch('/Api/FavoriteTeam', {
                    method: method,
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ teamId: teamId })
                });

                if (response.ok) {
                    const data = await response.json();
                    updateFavoriteButton(button, data.isFavorite);

                    // Show feedback animation
                    button.classList.add('favorite-animate');
                    setTimeout(() => button.classList.remove('favorite-animate'), 300);
                } else if (response.status === 401) {
                    // User not authenticated - show login modal
                    const loginModal = document.getElementById('loginModal');
                    if (loginModal) {
                        loginModal.classList.add('active');
                        document.body.style.overflow = 'hidden';
                    }
                } else {
                    throw new Error('Failed to toggle favorite');
                }
            } catch (error) {
                console.error('Error toggling favorite:', error);
                if (window.timezoneManager) {
                    window.timezoneManager.showNotification('Failed to favorite team. Please try again.');
                }
            } finally {
                button.disabled = false;
            }
        }

        // Attach event listeners
        favoriteButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleFavorite(this);
            });
        });

        // Initialize states on page load
        initializeFavoriteStates();
    })();

})();
