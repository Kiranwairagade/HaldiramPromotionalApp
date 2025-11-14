// Global notification system for all pages

document.addEventListener('DOMContentLoaded', function() {
    // Get DOM elements
    const notificationBell = document.getElementById('notificationBell');
    const notificationDropdown = document.getElementById('notificationDropdown');
    const notificationList = document.getElementById('notificationList');
    const notificationCount = document.getElementById('notificationCount');
    
    // Toggle notification dropdown
    if (notificationBell) {
        notificationBell.addEventListener('click', function(e) {
            e.stopPropagation();
            notificationDropdown.classList.toggle('hidden');
            
            // If opening the dropdown, fetch notifications
            if (!notificationDropdown.classList.contains('hidden')) {
                fetchNotifications();
            }
        });
    }
    
    // Close dropdown when clicking outside
    document.addEventListener('click', function(e) {
        if (notificationDropdown && !notificationDropdown.contains(e.target) && 
            notificationBell && !notificationBell.contains(e.target)) {
            notificationDropdown.classList.add('hidden');
        }
    });
    
    // Initial notification fetch
    if (notificationBell) {
        fetchNotifications();
    }
    
    // Set up periodic refresh (every 5 minutes)
    if (notificationBell) {
        setInterval(fetchNotifications, 5 * 60 * 1000);
    }
});

// Function to fetch notifications from the server
async function fetchNotifications() {
    try {
        const response = await fetch('/api/Notifications');
        const data = await response.json();
        
        if (data.success) {
            displayNotifications(data.notifications);
        } else {
            console.error('Failed to fetch notifications:', data.message);
            displayNotifications([]); // Clear notifications on error
        }
    } catch (error) {
        console.error('Error fetching notifications:', error);
        displayNotifications([]); // Clear notifications on error
    }
}

// Function to display notifications
function displayNotifications(notifications) {
    const notificationList = document.getElementById('notificationList');
    const notificationCount = document.getElementById('notificationCount');
    
    if (!notificationList || !notificationCount) return;
    
    if (!notifications || notifications.length === 0) {
        notificationList.innerHTML = '<div class="px-4 py-3 text-center text-gray-500 text-xs">No notifications</div>';
        notificationCount.textContent = '0';
        notificationCount.style.display = 'none';
        return;
    }
    
    // Update notification count
    notificationCount.textContent = notifications.length;
    notificationCount.style.display = notifications.length > 0 ? 'flex' : 'none';
    
    // Generate HTML for notifications
    let html = '';
    notifications.forEach(notification => {
        const timeAgo = getTimeAgo(notification.time);
        const typeClass = `badge-${notification.type}`;
        
        html += `
            <div class="notification-item unread" data-id="${notification.id}">
                <div class="flex justify-between items-start">
                    <span class="notification-badge ${typeClass}">${notification.badge}</span>
                    <span class="notification-time">${timeAgo}</span>
                </div>
                <h4 class="notification-title">${notification.title}</h4>
                <p class="notification-message">${notification.message}</p>
                <button class="notification-close" onclick="dismissNotification('${notification.id}')">×</button>
            </div>
        `;
    });
    
    notificationList.innerHTML = html;
}

// Helper function to calculate time ago
function getTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now - date;
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));
    
    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
    if (diffInHours < 24) return `${diffInHours}h ago`;
    if (diffInDays === 1) return '1d ago';
    if (diffInDays < 7) return `${diffInDays}d ago`;
    if (diffInDays < 30) return `${Math.floor(diffInDays / 7)}w ago`;
    return `${Math.floor(diffInDays / 30)}mo ago`;
}

// Function to dismiss a notification
function dismissNotification(id) {
    const notificationElement = document.querySelector(`.notification-item[data-id="${id}"]`);
    if (notificationElement) {
        notificationElement.remove();
        
        // Update notification count
        const notificationCount = document.getElementById('notificationCount');
        const currentCount = parseInt(notificationCount.textContent) || 0;
        if (currentCount > 0) {
            notificationCount.textContent = currentCount - 1;
            notificationCount.style.display = currentCount - 1 > 0 ? 'flex' : 'none';
        }
    }
}