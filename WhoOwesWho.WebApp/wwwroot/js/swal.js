window.blazorSwal = {
    showAdvanced: async function (message, type) {

        const config = {
            text: message,
            icon: type.toLowerCase(),
            confirmButtonText: "OK"
        };

        if (type === "confirmation") {
            config.showCancelButton = true;
            config.confirmButtonText = "Yes";
            config.cancelButtonText = "Cancel";
            config.icon = "question";
        }

        return await Swal.fire(config);
    }
};