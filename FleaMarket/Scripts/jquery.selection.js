 function ReplaceSelectedText() {
            if (typeof window.getSelection != "undefined") {
                // Ветка для Gecko-браузеров
                selected = window.getSelection();

                var selRange = selected.getRangeAt(0);

                var obj = document.createElement("div");
                obj.innerHTML = '<b>' + selRange.toString() + '</b>';
                obj.style.display = 'inline';

                selRange.deleteContents();
                selRange.insertNode(obj);
            }
            else {
                // Ветка для IE
                selected = document.selection.createRange();
                selected.text = '<b>' + selected.text + '</b>';
            }
        }