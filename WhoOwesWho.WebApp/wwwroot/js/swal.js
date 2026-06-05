window.blazorSwal = {
    showAdvanced: async function (message, type) {
        const isMobile = window.matchMedia("(max-width: 767px)").matches;
        const config = {
            text: message,
            confirmButtonText: "OK"
        };

        if (isMobile) {
            config.heightAuto = false;
            config.width = "80%";
            config.padding = "1.5rem";
        }

        if (type.toLowerCase() === "confirmation") {
            config.icon = "question";  // ← set here, not from type
            config.showCancelButton = true;
            config.confirmButtonText = "Yes";
            config.cancelButtonText = "Cancel";
        } else {
            config.icon = type.toLowerCase(); // ← only use type for non-confirmation
        }

        return await Swal.fire(config);
    }
};