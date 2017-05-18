var host = host || {};

host.alerts = (function () {
    var _alerts = [];

    var _alertContainer = $('.alerts');
    var _alertTemplate = _.template('<div class="alert <%= type %>"><%= message %></div>');

    var _addInfo = function (message, container, timeout) {
        _addAlert('alert-info', message, container, timeout);
    };

    var _addSuccess = function (message, container, timeout) {
        _addAlert('alert-success', message, container, timeout);
    };

    var _addWarning = function (message, container, timeout) {
        _addAlert('alert-warning', message, container, timeout);
    };

    var _addError = function (message, container, timeout) {
        _addAlert('alert-danger', message, container, timeout);
    };

    var _addAlert = function (type, message, container, timeout) {
        var alert = {
            type: type,
            message: message
        };

        if (!timeout) {
            timeout = 3000;
        }

        var targetContainer = container || _alertContainer;
        var alertElement = $(_alertTemplate(alert));
        targetContainer.append(alertElement);

        if (timeout > 0) {
            window.setTimeout(function () {
                alertElement.fadeOut();
            }, timeout);
        }

        _alerts.push(alert);
    };

    return {
        addInfo: _addInfo,
        addSuccess: _addSuccess,
        addWarning: _addWarning,
        addError: _addError
    };
}());