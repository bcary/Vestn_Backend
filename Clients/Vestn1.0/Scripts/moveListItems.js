jQuery.fn.moveUp = function () {
    var objectToMove = this[0];
    var objectToMoveTextNode = objectToMove.nextSibling;

    if (objectToMove.previousSibling.previousSibling == null) {
        return;
    }

    previousItemTextNode = objectToMove.previousSibling;
    previousItem = previousItemTextNode.previousSibling;

    var parentNode = objectToMove.parentNode;

    parentNode.insertBefore(objectToMoveTextNode, previousItem);
    parentNode.insertBefore(objectToMove, objectToMoveTextNode);
}

jQuery.fn.moveDown = function (index) {
    var objectToMove = this[0];
    var objectToMoveTextNode = objectToMove.nextSibling;

    if (objectToMove.nextSibling.nextSibling == null) {
        return;
    }

    nextItem = objectToMoveTextNode.nextSibling;
    nextItemTextNode = nextItem.nextSibling;

    var parentNode = objectToMove.parentNode;

    parentNode.insertBefore(objectToMoveTextNode, nextItemTextNode.nextSibling);
    parentNode.insertBefore(objectToMove, objectToMoveTextNode);
}