// Returns the bounding rect of an element, so C# can convert client coordinates
// into positions relative to it. Needed because the wheel event's offsetX/offsetY
// are relative to whichever element was hit, which may be a child of the map
// rather than the map container itself.
window.ratScanner = window.ratScanner || {};

window.ratScanner.getBoundingRect = function (element) {
  if (!element) return null;

  const rect = element.getBoundingClientRect();
  return {
    left: rect.left,
    top: rect.top,
    width: rect.width,
    height: rect.height,
  };
};
