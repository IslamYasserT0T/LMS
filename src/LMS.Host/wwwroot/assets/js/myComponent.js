window.myComponent = {
    configureQuillJs: function (quill) {
        // Force RTL on the root
        quill.root.setAttribute("dir", "rtl");
        quill.root.style.direction = "rtl";
        quill.root.style.textAlign = "right";

        // Optional: apply RTL whenever content changes
        quill.on('text-change', function () {
            quill.root.setAttribute("dir", "rtl");
            quill.root.style.direction = "rtl";
            quill.root.style.textAlign = "right";
        });
    }
};