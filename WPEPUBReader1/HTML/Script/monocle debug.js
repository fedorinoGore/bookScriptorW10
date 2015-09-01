var testObj = {
    testFunc: function () {
        "use strict";
        return {
            testVar: "bla-bla",
            testIncFunc: function () {}
        };
    }
};
var bookData = {
    getComponents: function () {
        "use strict";
        return ['cover.xhtml', 'title.xhtml', 'annotation.xhtml', 'section1.xhtml', 'section2.xhtml', 'section3.xhtml', 'section4.xhtml', 'section5.xhtml', 'section6.xhtml', 'section7.xhtml', 'section8.xhtml', 'section9.xhtml', 'section10.xhtml', 'section11.xhtml', 'section12.xhtml', 'section13.xhtml', 'section14.xhtml', 'section15.xhtml', 'section16.xhtml', 'section17.xhtml', 'section18.xhtml', 'section19.xhtml', 'section20.xhtml', 'section21.xhtml', 'section22.xhtml', 'section23.xhtml', 'section24.xhtml', 'section25.xhtml', 'section26.xhtml', 'section27.xhtml', 'section28.xhtml', 'section29.xhtml', 'section30.xhtml', 'section31.xhtml', 'section32.xhtml', 'section33.xhtml', 'section34.xhtml', 'section35.xhtml', 'section36.xhtml', 'section37.xhtml', 'section38.xhtml', 'section39.xhtml', 'section40.xhtml', 'section41.xhtml', 'section42.xhtml', 'section43.xhtml', 'section44.xhtml', 'section45.xhtml', 'section46.xhtml', 'section47.xhtml', 'section48.xhtml', 'section49.xhtml', 'section50.xhtml', 'section51.xhtml', 'section52.xhtml', 'section53.xhtml', 'section54.xhtml', 'section55.xhtml', 'section56.xhtml', 'section57.xhtml', 'section58.xhtml', 'section59.xhtml', 'section60.xhtml', 'section61.xhtml', 'section62.xhtml', 'section63.xhtml', 'section64.xhtml', 'section65.xhtml', 'section66.xhtml', 'section67.xhtml', 'section68.xhtml', 'section69.xhtml', 'section70.xhtml', 'section71.xhtml', 'section72.xhtml', 'section73.xhtml', 'section74.xhtml', 'section75.xhtml', 'section76.xhtml', 'section77.xhtml', 'section78.xhtml', 'section79.xhtml', 'section80.xhtml', 'section81.xhtml', 'section82.xhtml', 'section83.xhtml', 'section84.xhtml', 'section85.xhtml', 'section86.xhtml', 'section87.xhtml', 'section88.xhtml', 'section89.xhtml', 'section90.xhtml', 'section91.xhtml', 'section92.xhtml', 'section93.xhtml', 'section94.xhtml', 'section95.xhtml', 'section96.xhtml', 'section97.xhtml', 'section98.xhtml', 'section99.xhtml', 'section100.xhtml', 'section101.xhtml', 'section102.xhtml', 'section103.xhtml', 'section104.xhtml', 'section105.xhtml', 'section106.xhtml', 'section107.xhtml', 'section108.xhtml', 'section109.xhtml', 'section110.xhtml', 'section111.xhtml', 'section112.xhtml', 'section113.xhtml', 'section114.xhtml', 'section115.xhtml', 'section116.xhtml', 'section117.xhtml', 'section118.xhtml', 'section119.xhtml', 'section120.xhtml', 'section121.xhtml', 'section122.xhtml', 'section123.xhtml', 'section124.xhtml', 'section125.xhtml', 'section126.xhtml', 'section127.xhtml', 'section128.xhtml', 'section129.xhtml', 'section130.xhtml', 'section131.xhtml', 'section132.xhtml', 'section133.xhtml', 'section134.xhtml', 'section135.xhtml', 'section136.xhtml', 'section137.xhtml', 'section138.xhtml', 'section139.xhtml', 'section140.xhtml', 'section141.xhtml', 'section142.xhtml', 'section143.xhtml', 'section144.xhtml', 'section145.xhtml', 'section146.xhtml', 'section147.xhtml', 'section148.xhtml', 'section149.xhtml', 'section150.xhtml', 'section151.xhtml', 'section152.xhtml', 'section153.xhtml', 'section154.xhtml', 'section155.xhtml', 'section156.xhtml', 'section157.xhtml', 'section158.xhtml', 'section159.xhtml', 'section160.xhtml', 'section161.xhtml', 'section162.xhtml', 'section163.xhtml', 'section164.xhtml', 'section165.xhtml', 'section166.xhtml', 'section167.xhtml', 'section168.xhtml', 'section169.xhtml', 'section170.xhtml', 'section171.xhtml', 'section172.xhtml', 'section173.xhtml', 'section174.xhtml', 'section175.xhtml', 'section176.xhtml', 'section177.xhtml', 'section178.xhtml', 'section179.xhtml', 'section180.xhtml', 'section181.xhtml', 'section182.xhtml', 'section183.xhtml', 'section184.xhtml', 'section185.xhtml', 'section186.xhtml', 'section187.xhtml', 'section188.xhtml', 'section189.xhtml', 'section190.xhtml', 'section191.xhtml', 'section192.xhtml', 'section193.xhtml', 'section194.xhtml', 'section195.xhtml', 'section196.xhtml', 'section197.xhtml', 'section198.xhtml', 'section199.xhtml', 'section200.xhtml', 'section201.xhtml', 'section202.xhtml', 'section203.xhtml', 'section204.xhtml', 'section205.xhtml', 'section206.xhtml', 'section207.xhtml', 'section208.xhtml', 'section209.xhtml', 'section210.xhtml', 'section211.xhtml', 'section212.xhtml', 'section213.xhtml', 'section214.xhtml', 'section215.xhtml', 'section216.xhtml', 'section217.xhtml', 'section218.xhtml', 'section219.xhtml', 'section220.xhtml', 'section221.xhtml', 'section222.xhtml', 'section223.xhtml', 'section224.xhtml', 'section225.xhtml', 'section226.xhtml', 'section227.xhtml', 'section228.xhtml', 'section229.xhtml', 'section230.xhtml', 'section231.xhtml', 'section232.xhtml', 'section233.xhtml', 'section234.xhtml', 'section235.xhtml', 'section236.xhtml', 'section237.xhtml', 'section238.xhtml', 'section239.xhtml', 'section240.xhtml', 'section241.xhtml', 'section242.xhtml', 'section243.xhtml', 'section244.xhtml', 'section245.xhtml', 'section246.xhtml', 'section247.xhtml', 'section248.xhtml', 'section249.xhtml', 'section250.xhtml', 'section251.xhtml', 'section252.xhtml', 'section253.xhtml', 'section254.xhtml', 'section255.xhtml', 'section256.xhtml', 'section257.xhtml', 'section258.xhtml', 'section259.xhtml', 'section260.xhtml', 'section261.xhtml', 'section262.xhtml', 'section263.xhtml', 'section264.xhtml', 'section265.xhtml', 'section266.xhtml', 'section267.xhtml', 'section268.xhtml', 'section269.xhtml', 'section270.xhtml', 'section271.xhtml', 'section272.xhtml', 'section273.xhtml', 'section274.xhtml', 'section275.xhtml', 'section276.xhtml', 'section277.xhtml', 'section278.xhtml', 'section279.xhtml', 'section280.xhtml', 'section281.xhtml', 'section282.xhtml', 'section283.xhtml', 'section284.xhtml', 'section285.xhtml', 'section286.xhtml', 'section287.xhtml', 'section288.xhtml', 'section289.xhtml', 'section290.xhtml', 'section291.xhtml', 'section292.xhtml', 'section293.xhtml', 'section294.xhtml', 'section295.xhtml', 'section296.xhtml', 'section297.xhtml', 'section298.xhtml', 'section299.xhtml', 'section300.xhtml', 'section301.xhtml', 'section302.xhtml', 'section303.xhtml', 'section304.xhtml', 'section305.xhtml', 'section306.xhtml', 'section307.xhtml', 'section308.xhtml', 'section309.xhtml', 'section310.xhtml', 'section311.xhtml', 'section312.xhtml', 'section313.xhtml', 'section314.xhtml', 'section315.xhtml', 'section316.xhtml', 'section317.xhtml', 'section318.xhtml', 'section319.xhtml', 'section320.xhtml', 'section321.xhtml', 'section322.xhtml', 'section323.xhtml', 'section324.xhtml', 'section325.xhtml', 'section326.xhtml', 'section327.xhtml', 'section328.xhtml', 'section329.xhtml', 'section330.xhtml', 'section331.xhtml', 'section332.xhtml', 'section333.xhtml', 'section334.xhtml', 'section335.xhtml', 'section336.xhtml', 'section337.xhtml', 'section338.xhtml', 'section339.xhtml', 'section340.xhtml', 'section341.xhtml', 'section342.xhtml', 'section343.xhtml', 'section344.xhtml', 'section345.xhtml', 'section346.xhtml', 'section347.xhtml', 'section348.xhtml', 'section349.xhtml', 'section350.xhtml', 'section351.xhtml', 'section352.xhtml', 'section353.xhtml', 'section354.xhtml', 'section355.xhtml', 'section356.xhtml', 'section357.xhtml', 'section358.xhtml', 'section359.xhtml', 'section360.xhtml', 'section361.xhtml', 'section362.xhtml', 'section363.xhtml', 'section364.xhtml', 'section365.xhtml', 'section366.xhtml', 'section367.xhtml', 'section368.xhtml', 'section369.xhtml', 'section370.xhtml', 'section371.xhtml', 'section372.xhtml', 'section373.xhtml', 'section374.xhtml', 'section375.xhtml', 'section376.xhtml', 'section377.xhtml', 'section378.xhtml', 'section379.xhtml', 'section380.xhtml', 'section381.xhtml', 'section382.xhtml', 'section383.xhtml', 'section384.xhtml', 'section385.xhtml', 'section386.xhtml', 'section387.xhtml', 'section388.xhtml', 'section389.xhtml', 'section390.xhtml', 'section391.xhtml', 'section392.xhtml', 'section393.xhtml', 'section394.xhtml', 'section395.xhtml', 'section396.xhtml', 'section397.xhtml', 'section398.xhtml', 'section399.xhtml', 'section400.xhtml', 'section401.xhtml', 'section402.xhtml', 'section403.xhtml', 'section404.xhtml', 'section405.xhtml', 'section406.xhtml', 'section407.xhtml', 'section408.xhtml', 'section409.xhtml', 'section410.xhtml', 'section411.xhtml', 'section412.xhtml', 'section413.xhtml', 'section414.xhtml', 'section415.xhtml', 'section416.xhtml', 'section417.xhtml', 'section418.xhtml', 'section419.xhtml', 'section420.xhtml', 'section421.xhtml', 'section422.xhtml', 'section423.xhtml', 'section424.xhtml', 'section425.xhtml', 'section426.xhtml', 'section427.xhtml', 'section428.xhtml', 'section429.xhtml', 'section430.xhtml', 'section431.xhtml', 'section432.xhtml', 'section433.xhtml', 'section434.xhtml', 'section435.xhtml', 'section436.xhtml', 'section437.xhtml', 'section438.xhtml', 'section439.xhtml', 'section440.xhtml', 'section441.xhtml', 'section442.xhtml', 'section443.xhtml', 'section444.xhtml', 'section445.xhtml', 'section446.xhtml', 'section447.xhtml', 'section448.xhtml', 'section449.xhtml', 'section450.xhtml', 'section451.xhtml', 'section452.xhtml', 'section453.xhtml', 'section454.xhtml', 'section455.xhtml', 'section456.xhtml', 'section457.xhtml', 'section458.xhtml', 'section459.xhtml', 'section460.xhtml', 'section461.xhtml', 'section462.xhtml', 'section463.xhtml', 'section464.xhtml', 'section465.xhtml', 'section466.xhtml', 'section467.xhtml', 'section468.xhtml', 'section469.xhtml', 'section470.xhtml', 'section471.xhtml', 'section472.xhtml', 'section473.xhtml', 'section474.xhtml', 'section475.xhtml', 'section476.xhtml', 'section477.xhtml', 'section478.xhtml', 'section479.xhtml', 'section480.xhtml', 'section481.xhtml', 'section482.xhtml', 'section483.xhtml', 'section484.xhtml', 'section485.xhtml', 'section486.xhtml', 'section487.xhtml', 'section488.xhtml', 'section489.xhtml', 'section490.xhtml', 'section491.xhtml', 'fb2info.xhtml', 'about.xhtml'];
    },
    getContents: function () {
        "use strict";
        return [{
            title: "main",
            src: 'section1.xhtml',
            children: [{
                    title: "Этимология",
                    src: 'section2.xhtml'
                }, {
                    title: "Глава CXXXIII. Погоня, день первый",
                    src: 'section136.xhtml'
                },
                {
                    title: "Глава CXXXIV. Погоня, день второй",
                    src: 'section137.xhtml'
                },
                {
                    title: "Глава CXXXV. Погоня, день третий",
                    src: 'section138.xhtml'
                },
                {
                    title: "ЭПИЛОГ",
                    src: 'section139.xhtml'
                }]
                }, {
            title: "Словарь",
            src: 'section140.xhtml',
            children: [
                {
                    title: "Словарь морских терминов",
                    src: 'section141.xhtml'
                }]
                }, {
            title: "Критика",
            src: 'section144.xhtml',
            children: [
                {
                    title: "Герман Мелвилл и его «Моби Дик»",
                    src: 'section145.xhtml'
                },
                {
                    title: "Послесловие к роману «Моби Дик, или Белый кит»",
                    src: 'section146.xhtml'
                },
                {
                    title: "Роман о Белом ките",
                    src: 'section147.xhtml'
                },
                {
                    title: "«Лицом к лицу встречаю я тебя сегодня, о Моби Дик!»",
                    src: 'section148.xhtml'
                },
                {
                    title: "Пределы вселенной Германа Мелвилла",
                    src: 'section149.xhtml'
                },
                {
                    title: "Послесловие к роману Г. Мелвилла «Моби Дик, или Белый кит»",
                    src: 'section150.xhtml'
                }]
                }];
    },
    getComponent: function (componentId) {
        return {
            url: componentId
        };
    },
    ;;
    getMetaData: function (key) {
        return {
            title: "Test document",
            creator: "Aron Woost"
        }[key];
    }
};
Monocle.Events.listen(window, 'load', function () {
    window.reader = Monocle.Reader('reader', bookData);
});
