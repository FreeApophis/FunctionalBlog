// Keyboard navigation for the nav quicksearch dropdown.
//
// The dropdown items (rendered by SearchViews.QuickResults into #quicksearch-results)
// are plain anchors, so they are already focusable. This wires Arrow keys so the user
// can walk from the search input down into the results and back up again:
//
//   - ArrowDown in the input focuses the first result.
//   - ArrowDown / ArrowUp move between results, wrapping back to the input at the edges.
//   - Escape returns focus to the input.
//
// Listening on document (event delegation) means freshly htmx-swapped results work without
// re-binding.
(function () {
    "use strict";

    var ITEM = "a.quicksearch-item";

    function items(box) {
        var panel = box.querySelector(".quicksearch-results");
        return panel ? Array.prototype.slice.call(panel.querySelectorAll(ITEM)) : [];
    }

    function input(box) {
        return box.querySelector('input[name="q"]');
    }

    document.addEventListener("keydown", function (e) {
        if (e.key !== "ArrowDown" && e.key !== "ArrowUp" && e.key !== "Escape") {
            return;
        }

        var box = e.target.closest ? e.target.closest(".search-box") : null;
        if (!box) {
            return;
        }

        var field = input(box);
        var list = items(box);

        if (e.target === field) {
            if (e.key === "ArrowDown" && list.length > 0) {
                e.preventDefault();
                list[0].focus();
            } else if (e.key === "Escape") {
                field.blur();
            }
            return;
        }

        var idx = list.indexOf(e.target);
        if (idx === -1) {
            return;
        }

        if (e.key === "Escape") {
            field.focus();
            return;
        }

        e.preventDefault();
        var next = e.key === "ArrowDown" ? idx + 1 : idx - 1;
        if (next < 0 || next >= list.length) {
            field.focus();
        } else {
            list[next].focus();
        }
    });
}());
