import '../scss/styles.scss';
(function($) {
    'use strict';

    /**
     * Табы
     */
    $.fn.tabs = function () {
        var $self = $(this);
        var $tabHeaders = $self.find('.js-tab-header').filter(function (index, el) {
            return $(el).parentsUntil($self).length === 1;
        });
        var $tabContent = $self.find('.js-tab-content').filter(function (index, el) {
            return $(el).parentsUntil($self).length === 1;
        });

        /**
         * Активация таба по его индексу
         * @param {Number} index - индекс таба, который нужно активировать
         */
        var selectTab = function (index) {
            $tabHeaders.removeClass('active').eq(index).addClass('active');
            $tabContent.removeClass('active').eq(index).addClass('active');
        };

        /**
         * Инициализаиця
         */
        var init = function () {
            selectTab(0);

            // Обработка событий
            $tabHeaders.on('click', function () {
                selectTab($(this).index());
            });
        };

        init();

        this.selectTab = selectTab;

        return this;
    };

    // Инициализируем табы на всех блоках с классом 'js-tabs'
    $('.js-tabs').each(function () {
        $(this).data('tabs', $(this).tabs());
    });

    // Добавляем анимацию для header при скролле страницы
    $(window).scroll(function() {
        if ($(this).scrollTop() > 1) {
            $('header').animate({
                height: 70
            }, 0.999, "linear");
        }
        else if($(this).scrollTop() === 0){
            $('header').animate({
                height: 160
            }, 0.999, "linear");
        }

    });
    $(document).ready(function () {
        console.log("1");
        if (screen.width <= 768) {

            $('#menu > li').click(function (event) {
                $('.tab-header-wrap').addClass('mobile')
                $(this).children("ul").slideToggle();
                event.stopPropagation();
                $('#pList').text($(this).children("ul").children('li.active').html());
            });
        };
    });
    /*$('.input').click(function(){

    })*/
    /*$('#example-three').hierarchySelect({
        hierarchy: false,
        search: false,
        width: 200
    });*/

    // $(document).ready(function () {
    //     $('#menu > li').click(function (event) {
    //         $(this).children("ul").slideToggle();
    //         event.stopPropagation();
    //     });
    // });

    /*$(document).ready(function(event) {
        if($(window).width(event) <= 768) {
            /!*$('.tab-header-wrap > ul').addClass("mobileTabs");
            $('.tab-header-wrap > ul').click(function (event) {
                $(this).children("li").slideToggle();
                event.stopPropagation();
            });*!/
        }
    })*/

    $( function() {
        $( "#draggable" ).draggable();
    } );

    $( function() {
        $( "#resizable" ).resizable();
    } );



    jQuery("#movephoto").mousemove(
        function(e){
            var offset = jQuery(this).offset();
            var xPos = e.pageX - offset.left;
            var yPos = e.pageY - offset.top;
            var mouseXPercent = Math.round(xPos / jQuery(this).width() * 1000);
            var mouseYPercent = Math.round(yPos / jQuery(this).height() * 1000);
            jQuery(this).children('img').each(
                function(){
                    var diffX = jQuery('#Parallax').width() - jQuery(this).width();
                    var diffY = jQuery('#Parallax').height() - jQuery(this).height();
                    var myX = diffX * (mouseXPercent / 1500);
                    var myY = diffY * (mouseYPercent / 1080);
                    var cssObj = {
                        'left': myX + 'px',
                        'top': myY + 'px'
                    }
                    jQuery(this).animate({left: myX, top: myY},{duration: 50, queue: false, easing: 'linear'});
                }
            );
        }
    );

})(jQuery);
