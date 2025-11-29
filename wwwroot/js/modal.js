// Sistema de modales estilo shadcn
window.Modal = {
    open: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            console.log('🔓 Abriendo modal:', modalId);
            modal.classList.remove('hidden');
            document.body.classList.add('modal-open');
            
            // Agregar animación de entrada
            setTimeout(() => {
                modal.querySelector('.modal-content')?.classList.add('modal-open');
            }, 10);
        } else {
            console.error('❌ Modal no encontrado:', modalId);
        }
    },
    
    close: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            console.log('🔒 Cerrando modal:', modalId);
            const content = modal.querySelector('.modal-content');
            content?.classList.remove('modal-open');
            
            setTimeout(() => {
                modal.classList.add('hidden');
                document.body.classList.remove('modal-open');
            }, 200);
        } else {
            console.error('❌ Modal no encontrado para cerrar:', modalId);
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
        if (openModal && openModal.id !== 'crudModal') {
            Modal.close(openModal.id);
        }
    }
});
