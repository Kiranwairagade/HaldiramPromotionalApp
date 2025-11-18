// Global notification system for all pages

document.addEventListener('DOMContentLoaded', function() {
    // Get DOM elements
    const notificationBell = document.getElementById('notificationBell');
    const notificationDropdown = document.getElementById('notificationDropdown');
    const notificationList = document.getElementById('notificationList');
    const notificationCount = document.getElementById('notificationCount');
    
    // Add new button elements
    const markAllReadBtn = document.getElementById('markAllRead');
    const deleteAllBtn = document.getElementById('deleteAll');
    const closeNotificationsBtn = document.getElementById('closeNotifications');
    
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
    
    // Mark all notifications as read
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            markAllNotificationsAsRead();
        });
    }
    
    // Delete all notifications
    if (deleteAllBtn) {
        deleteAllBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            deleteAllNotifications();
        });
    }
    
    // Close notifications dropdown
    if (closeNotificationsBtn) {
        closeNotificationsBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            if (notificationDropdown) {
                notificationDropdown.classList.add('hidden');
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
    
    // Generate responsive HTML for notifications with clear action buttons on the right
    let html = '';
    notifications.forEach(notification => {
        const timeAgo = getTimeAgo(notification.time);
        const badgeText = notification.badge || '';

        html += `
            <div class="notification-item unread px-3 py-3 flex items-start gap-3 hover:bg-gray-50 text-sm" data-id="${notification.id}">
                <div class="flex-shrink-0 mt-1">
                    <span class="inline-flex items-center justify-center px-2 py-1 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800">${badgeText}</span>
                </div>
                <div class="flex-1 min-w-0">
                    <div class="flex items-start justify-between gap-3">
                        <div class="min-w-0">
                            <h4 class="text-sm font-semibold text-gray-800 truncate">${notification.title}</h4>
                            <p class="text-sm text-gray-600 mt-1">${notification.message}</p>
                            <div class="text-xs text-gray-400 mt-2">${timeAgo}</div>
                        </div>
                        <div class="ml-3 flex-shrink-0 flex flex-col items-end gap-2">
                            <button onclick="markNotificationAsRead('${notification.id}')" class="px-3 py-1  text-black rounded-md text-xs flex items-center" title="Mark as read">
                                <i class="fas fa-check mr-2"></i>
                            </button>
                            <button onclick="deleteNotification('${notification.id}')" class="px-3 py-1  text-black rounded-md text-xs flex items-center" title="Delete">
                                <i class="fas fa-trash mr-2"></i>
                            </button>
                        </div>
                    </div>
                </div>
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

// Function to mark a single notification as read
function markNotificationAsRead(id) {
    const notificationElement = document.querySelector(`.notification-item[data-id="${id}"]`);
    if (!notificationElement) return;

    // Optimistically update UI
    notificationElement.classList.remove('unread');

    // Call API to mark as read
    (async () => {
        try {
            const payload = [parseInt(id)];
            const res = await fetch('/api/Notifications/MarkAsRead', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const data = await res.json();
            if (!data.success) {
                console.error('Mark as read failed:', data.message);
            }
        } catch (err) {
            console.error('Error marking notification as read:', err);
        } finally {
            // Update notification count locally
            const notificationCount = document.getElementById('notificationCount');
            const currentCount = parseInt(notificationCount.textContent) || 0;
            if (currentCount > 0) {
                const next = currentCount - 1;
                notificationCount.textContent = next.toString();
                notificationCount.style.display = next > 0 ? 'flex' : 'none';
            }
        }
    })();
}

// Function to delete a single notification
function deleteNotification(id) {
    const notificationElement = document.querySelector(`.notification-item[data-id="${id}"]`);
    if (!notificationElement) return;

    (async () => {
        try {
            const payload = [parseInt(id)];
            const res = await fetch('/api/Notifications/Delete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const data = await res.json();
            if (data.success) {
                notificationElement.remove();
                // Update notification count
                const notificationCount = document.getElementById('notificationCount');
                const currentCount = parseInt(notificationCount.textContent) || 0;
                if (currentCount > 0) {
                    const next = currentCount - 1;
                    notificationCount.textContent = next.toString();
                    notificationCount.style.display = next > 0 ? 'flex' : 'none';
                }
            } else {
                console.error('Delete failed:', data.message);
            }
        } catch (err) {
            console.error('Error deleting notification:', err);
        }
    })();
}

// Function to mark all notifications as read
function markAllNotificationsAsRead() {
    const notificationItems = Array.from(document.querySelectorAll('.notification-item.unread'));
    if (notificationItems.length === 0) return;

    const ids = notificationItems.map(i => parseInt(i.getAttribute('data-id'))).filter(Boolean);
    // Optimistically update UI
    notificationItems.forEach(i => i.classList.remove('unread'));

    (async () => {
        try {
            const res = await fetch('/api/Notifications/MarkAsRead', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(ids)
            });
            const data = await res.json();
            if (!data.success) console.error('Mark all read failed:', data.message);
        } catch (err) {
            console.error('Error marking all notifications as read:', err);
        } finally {
            const notificationCount = document.getElementById('notificationCount');
            if (notificationCount) {
                notificationCount.textContent = '0';
                notificationCount.style.display = 'none';
            }
        }
    })();
}

// Function to delete all notifications
function deleteAllNotifications() {
    const notificationList = document.getElementById('notificationList');
    if (!notificationList) return;

    // Collect all notification ids; if none, still call server to clear
    const items = Array.from(document.querySelectorAll('.notification-item'));
    const ids = items.map(i => parseInt(i.getAttribute('data-id'))).filter(Boolean);

    (async () => {
        try {
            const res = await fetch('/api/Notifications/Delete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(ids)
            });
            const data = await res.json();
            if (data.success) {
                notificationList.innerHTML = '<div class="px-4 py-3 text-center text-gray-500 text-xs">No notifications</div>';
                const notificationCount = document.getElementById('notificationCount');
                if (notificationCount) {
                    notificationCount.textContent = '0';
                    notificationCount.style.display = 'none';
                }
            } else {
                console.error('Delete all failed:', data.message);
            }
        } catch (err) {
            console.error('Error deleting all notifications:', err);
        }
    })();
}
