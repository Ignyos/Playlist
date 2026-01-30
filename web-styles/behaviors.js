/**
 * Ignyos Behaviors (web)
 * Interactive component APIs for modals, alerts, and toasts.
 * Requires behaviors.css and tokens.css
 */

const IgModal = (() => {
    let activeModal = null;

    function show(title, content, options = {}) {
        if (activeModal) {
            close();
        }

        const backdrop = document.createElement('div');
        backdrop.className = 'ig-modal-backdrop';
        
        const modal = document.createElement('div');
        modal.className = 'ig-modal';
        
        const header = document.createElement('div');
        header.className = 'ig-modal-header';
        
        const titleEl = document.createElement('h2');
        titleEl.className = 'ig-modal-title';
        titleEl.textContent = title;
        
        const closeBtn = document.createElement('button');
        closeBtn.className = 'ig-modal-close';
        closeBtn.innerHTML = '×';
        closeBtn.setAttribute('aria-label', 'Close');
        closeBtn.onclick = () => close();
        
        header.appendChild(titleEl);
        header.appendChild(closeBtn);
        
        const body = document.createElement('div');
        body.className = 'ig-modal-body';
        if (typeof content === 'string') {
            body.innerHTML = content;
        } else {
            body.appendChild(content);
        }
        
        modal.appendChild(header);
        modal.appendChild(body);
        
        if (options.footer || options.buttons) {
            const footer = document.createElement('div');
            footer.className = 'ig-modal-footer';
            
            if (options.buttons) {
                options.buttons.forEach(btn => {
                    const button = document.createElement('button');
                    button.className = 'ig-pill';
                    button.textContent = btn.text;
                    button.style.cursor = 'pointer';
                    button.style.fontFamily = 'inherit';
                    button.style.fontSize = 'inherit';
                    button.onclick = () => {
                        if (btn.onClick) btn.onClick();
                        if (btn.close !== false) close();
                    };
                    footer.appendChild(button);
                });
            } else if (options.footer) {
                footer.appendChild(options.footer);
            }
            
            modal.appendChild(footer);
        }
        
        backdrop.appendChild(modal);
        document.body.appendChild(backdrop);
        activeModal = backdrop;
        
        // Close on backdrop click
        backdrop.onclick = (e) => {
            if (e.target === backdrop && options.closeOnBackdrop !== false) {
                close();
            }
        };
        
        // Close on ESC
        const handleEscape = (e) => {
            if (e.key === 'Escape' && options.closeOnEscape !== false) {
                close();
            }
        };
        document.addEventListener('keydown', handleEscape);
        backdrop.dataset.escapeHandler = 'attached';
        
        // Animate in
        setTimeout(() => backdrop.classList.add('show'), 10);
        
        return { close };
    }

    function close() {
        if (!activeModal) return;
        
        activeModal.classList.remove('show');
        setTimeout(() => {
            if (activeModal) {
                activeModal.remove();
                activeModal = null;
            }
        }, 200);
        
        // Remove escape listener
        document.removeEventListener('keydown', handleEscape);
    }

    return { show, close };
})();

const IgAlert = (() => {
    function create(message, options = {}) {
        const { type = 'info', title, dismissible = true, container } = options;
        
        const alert = document.createElement('div');
        alert.className = `ig-alert ${type}`;
        
        // Icon
        const icon = document.createElement('div');
        icon.className = 'ig-alert-icon';
        icon.innerHTML = getIcon(type);
        alert.appendChild(icon);
        
        // Content
        const content = document.createElement('div');
        content.className = 'ig-alert-content';
        
        if (title) {
            const titleEl = document.createElement('div');
            titleEl.className = 'ig-alert-title';
            titleEl.textContent = title;
            content.appendChild(titleEl);
        }
        
        const messageEl = document.createElement('div');
        messageEl.className = 'ig-alert-message';
        messageEl.textContent = message;
        content.appendChild(messageEl);
        
        alert.appendChild(content);
        
        // Close button
        if (dismissible) {
            const closeBtn = document.createElement('button');
            closeBtn.className = 'ig-alert-close';
            closeBtn.innerHTML = '×';
            closeBtn.setAttribute('aria-label', 'Dismiss');
            closeBtn.onclick = () => {
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 200);
            };
            alert.appendChild(closeBtn);
        }
        
        // Add to container
        const targetContainer = container || document.body;
        targetContainer.appendChild(alert);
        
        return alert;
    }

    function info(message, options = {}) {
        return create(message, { ...options, type: 'info' });
    }

    function success(message, options = {}) {
        return create(message, { ...options, type: 'success' });
    }

    function warning(message, options = {}) {
        return create(message, { ...options, type: 'warning' });
    }

    function error(message, options = {}) {
        return create(message, { ...options, type: 'error' });
    }

    function getIcon(type) {
        const icons = {
            info: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/><path d="m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533L8.93 6.588zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0z"/></svg>',
            success: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/></svg>',
            warning: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/></svg>',
            error: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/></svg>'
        };
        return icons[type] || icons.info;
    }

    return { create, info, success, warning, error };
})();

const IgMsg = (() => {
    const containers = {};

    function getContainer(position = 'tr') {
        if (!containers[position]) {
            const container = document.createElement('div');
            container.className = `ig-toast-container ${position}`;
            document.body.appendChild(container);
            containers[position] = container;
        }
        return containers[position];
    }

    function show(message, options = {}) {
        const { type = 'info', duration = 3000, position = 'tr' } = options;
        
        const toastContainer = getContainer(position);
        
        const toast = document.createElement('div');
        toast.className = `ig-toast ${type}`;
        
        // Icon
        const icon = document.createElement('div');
        icon.className = 'ig-toast-icon';
        icon.innerHTML = getIcon(type);
        toast.appendChild(icon);
        
        // Message
        const messageEl = document.createElement('div');
        messageEl.className = 'ig-toast-message';
        messageEl.textContent = message;
        toast.appendChild(messageEl);
        
        toastContainer.appendChild(toast);
        
        // Click to dismiss
        toast.style.cursor = 'pointer';
        toast.onclick = () => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 200);
        };
        
        // Animate in
        setTimeout(() => toast.classList.add('show'), 10);
        
        // Auto-dismiss
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 200);
        }, duration);
        
        return toast;
    }

    function info(message, options = {}) {
        return show(message, { ...options, type: 'info' });
    }

    function success(message, options = {}) {
        return show(message, { ...options, type: 'success' });
    }

    function warning(message, options = {}) {
        return show(message, { ...options, type: 'warning' });
    }

    function error(message, options = {}) {
        return show(message, { ...options, type: 'error' });
    }

    function getIcon(type) {
        const icons = {
            info: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/><path d="m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533L8.93 6.588zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0z"/></svg>',
            success: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/></svg>',
            warning: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/></svg>',
            error: '<svg fill="currentColor" viewBox="0 0 16 16"><path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/></svg>'
        };
        return icons[type] || icons.info;
    }

    return { show, info, success, warning, error };
})();
