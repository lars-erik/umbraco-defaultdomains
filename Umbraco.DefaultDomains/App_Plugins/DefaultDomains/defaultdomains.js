(function() {
    
    function DefaultDomainsController(scope, http) {
        var content;

        function findContent(curScope) {
            return curScope.content || findContent(curScope.$parent);
        }

        content = findContent(scope);
        scope.domains = [];

        http.get("/umbraco/backoffice/api/defaultdomains/getdomains", {
            params: {
                id: content.id
            }})
            .success(function (domains) {
                domains.splice(0, 0, "");
                scope.domains = domains;
            });

    }

    angular.module("umbraco").controller("defaultDomainsController", ["$scope", "$http", DefaultDomainsController]);

}());
