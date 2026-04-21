window.blazorSwal = {
    showAdvanced: async function (message, type) {
        const config = {
            text: message,
            confirmButtonText: "OK"
        };

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