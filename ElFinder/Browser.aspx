<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Browser.aspx.cs" Inherits="DNNConnect.CKEditorProvider.ElFinder.Browser" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/Portals/_default/default.css" rel="stylesheet" />
    <link href="~/Providers/HtmlEditorProviders/DNNConnect.CKE/ElFinder/css/reset.css" rel="stylesheet" />
    <link href="~/Providers/HtmlEditorProviders/DNNConnect.CKE/ElFinder/css/elfinder.full.css" rel="stylesheet" />
    <link href="~/Providers/HtmlEditorProviders/DNNConnect.CKE/ElFinder/css/dnn.theme.css" rel="stylesheet" />
    <script type="text/javascript" src='<%=ResolveUrl("~/Resources/Libraries/jQuery/01_09_01/jquery.js") %>'></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/Resources/Libraries/jQuery-UI/01_11_03/jquery-ui.js") %>'></script>
    <script type="text/javascript" src='<%=ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ElFinder/js/elfinder.full.js") %>'></script>
    <script type="text/javascript" src='<%=LanguageFileUrl %>'></script>
</head>
<body>
    <form id="form1" runat="server" class="dnnForm">
        <div id="elfinder"></div>
        <div class="elfinder-actions">
            <button type="button" id="elfinder-selectFile">Select</button>&nbsp;
            <button type="button" id="elfinder-close">Close</button>
        </div>
        <script type="text/javascript">
            $(function () {

                if ($(document.body).css('direction') == rtl)
                    $(document.body).addClass('rtl');

                var selectedFile = null;
                var fileManager = null;
                var options = <%= ElOptions %>;

                var applySelectedFile = function () {
                    if (selectedFile != null){
                        fileManager.request({
                            // Issuing the custom 'desc' command, targetting the selected file
                            data : { cmd: 'url', target: selectedFile, },
                            preventDefault: true,
                        })                   
                        // If the request fails, populate the field with 'Unknown'
                        .fail(function() {
                            alert(fileManager.i18n('unknown'));
                        })
                        // When the request is successful, show the description
                        .done(function(data) {
                            var E = window.top.opener;
                            E.CKEDITOR.tools.callFunction(options.CKEditorFuncNum, data.url,'');
                            self.close();
                        });
                    }
                };
		
				var actionsHeight = $('.elfinder-actions').height() + 10;
				options.resizable = false;
				options.disableInfoLink = true;
                options.height = $(window).innerHeight() - actionsHeight;
                options.handlers= {
                    select: function (event, elfinderInstance) {

                        if (event.data.selected.length == 1) {
                            var item = $('#' + event.data.selected[0]);
                            if (!item.hasClass('directory')) {
                                selectedFile = event.data.selected[0];
                                $('#elfinder-selectFile').prop('disabled', false);
                                fileManager =elfinderInstance; 
                                return;
                            }
                        }
                        $('#elfinder-selectFile').prop('disabled', true);
                        selectedFile = null;
                    },

                    dblclick: function() {
                        applySelectedFile();
                    }
                };
                
                fileManager = $('#elfinder').elfinder(options).elfinder('instance');
                
                $(document.body).css('direction', fileManager.direction);

                $('#elfinder-close').click(function () {
                    self.close();
                });

                $('#elfinder-selectFile').click(applySelectedFile);
                
                var ckLang = window.top.opener.CKEDITOR.lang[options.lang];

                if (ckLang){
                    $('#elfinder-selectFile').text(window.top.opener.CKEDITOR.lang[options.lang].table.cell.chooseColor);
                    $('#elfinder-close').text(fileManager.i18n('btnClose'));
                }

				// fit to window.height on window.resize
				var resizeTimer = null;
				$(window).resize(function() {
					resizeTimer && clearTimeout(resizeTimer);
					resizeTimer = setTimeout(function() {
						var h = parseInt($(window).height()) - actionsHeight;
						if (h != parseInt($('#elfinder').height())) {
							fileManager.resize('100%', h);
						}
					}, 200);
				});
            });
        </script>
    </form>
</body>
</html>
