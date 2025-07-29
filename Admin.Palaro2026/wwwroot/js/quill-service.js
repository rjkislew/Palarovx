window.quillInterop = {
    quillInstances: {},

    initializeQuill: function (elementId, dotNetHelper, content) {
        const editor = new Quill(`#${elementId}`, {
            theme: 'snow',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline'],
                    [{ 'header': 1 }, { 'header': 2 }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    ['link']
                ]
            }
        });

        if (content) {
            editor.root.innerHTML = content;
        }

        editor.on('text-change', function () {
            const html = editor.root.innerHTML;
            dotNetHelper.invokeMethodAsync('OnQuillContentChanged', html);
        });

        this.quillInstances[elementId] = editor;
    },

    getContent: function (elementId) {
        const editor = this.quillInstances[elementId];
        return editor ? editor.root.innerHTML : "";
    },

    setContent: function (elementId, content) {
        const editor = this.quillInstances[elementId];
        if (editor) {
            editor.root.innerHTML = content;
        }
    }
};
