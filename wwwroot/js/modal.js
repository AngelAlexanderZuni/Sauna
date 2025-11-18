// Sistema de modales estilo shadcn
const Modal = {
    open: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('hidden');
            document.body.classList.add('modal-open');
            
            // Agregar animación de entrada
            setTimeout(() => {
                modal.querySelector('.modal-content')?.classList.add('modal-open');
            }, 10);
        }
    },
    
    close: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            const content = modal.querySelector('.modal-content');
            content?.classList.remove('modal-open');
            
            setTimeout(() => {
                modal.classList.add('hidden');
                document.body.classList.remove('modal-open');
            }, 200);
        }
    },
    
    closeOnBackdrop: function(event, modalId) {
        if (event.target.id === modalId) {
            Modal.close(modalId);
        }
    }
};

// Cerrar modal con ESC
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        const openModal = document.querySelector('.modal-backdrop:not(.hidden)');
        if (openModal) {
            Modal.close(openModal.id);
        }
    }
});
