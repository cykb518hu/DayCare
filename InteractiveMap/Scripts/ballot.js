var jsonurl = "http://localhost:54817/map-ballots.json", questionId, questionData; location.search ? (questionId = location.search.substring(3, 4), $("#change-questions").val("ballot.html?q=" + questionId)) : ($("#change-questions").val("ballot.html?q=1"), questionId = 1); var towns = [], townbytown = {}, results = { towns: [], townids: [], precinctPercentage: [], precinctsReporting: [], precinctsTotal: [], colorcode: [], diff: [], winner: [], votecount: {}, votepct: {} }, tablebody = [], state, raceids = ["24805","24802", "24803", "24804" ], parties = ["Yes", "No"]; d3.json(jsonurl, function (a, b) { function c(a) { var b = topojson.feature(a, a.objects.towns), c = d3.geo.path().projection(null), d = d3.select("svg"), e = d.append("g").attr({ class: "g-town" }); e.selectAll("path.town").data(b.features).enter().append("path").attr({ "data-town": function (a, b) { return results.towns[b] }, class: function (a, b) { return results.colorcode[b] + " townshape" }, d: c, tabindex: function (a, b) { return b + 1 } }) } function d(a, b, c) { var d = b - 30 - $(".mass-map").offset().left, e = c - 30 - $(".mass-map").offset().top; $("#town-name").text(a).css("top", e).css("left", d).show() } function e() { $("#town-name").hide() } function f(a) { var b = questionData.towns[a]; for ($("#tooltiptable, .pctreported").show(), $(".infobox .townname").removeClass("introtext"), $(".infobox .townname").text(b.townName), $(".infobox .pctreportedpct").text(b.precinctPercentage + "%"), $(".infobox .pctreportedtotal").text(b.precinctsTotal), $(".infobox .pctreportednum").text(b.precinctsReporting), i = 0; i < parties.length; i++) $(".infobox ." + parties[i] + " .votecount").text("(" + d3.format(",")(b.candidates[parties[i]].votes) + ")"), $(".infobox ." + parties[i] + " .votepct").text(b.candidates[parties[i]].votePercent + "%"), $(".infobox ." + parties[i] + " .bar").css("width", 2.8 * b.candidates[parties[i]].votePercent) } function g(a) { for (i = 0; i < parties.length; i++) $("#statetable .mapct" + parties[i]).text(a.candidates[parties[i]].votePercent + "%"), $("#statetable .macount" + parties[i]).text(d3.format(",")(a.candidates[parties[i]].votes) + " votes"), "true" == a.candidates[parties[i]].isWinner && $("#statetable th." + parties[i]).append(" <i class='fa fa fa-check-circle'></i>"); $("#stateresults .mareportedpct").text(a.precinctPercentage + "%"), $("#stateresults .mareportednum").text(a.precinctsReporting), $("#stateresults .mareportedtotal").text(a.precinctsTotal) } function h(a, b) { var c = d3.select("#resultstable").append("tbody"), d = c.selectAll("tr").data(a.towns).enter().append("tr").attr("class", function (b, c) { return b + " " + a.colorcode[c] + " wonby" + a.winner[c] }); d.selectAll("td").data(function (a, b) { return tablebody[b] }).enter().append("td").text(function (a) { return a }).filter(function (a, b) { return b > 0 }).attr("align", "right").filter(function (a, b) { return b > 3 }).attr("class", "hide-for-small"); return c } for (questionData = b["raceid-" + raceids[questionId - 1]], _.map(questionData.towns, function (a) { return "Massachusetts" == a.townName ? void (state = a) : (towns.push(a.townName), results.towns.push(a.townName), results.townids.push(a.id), results.precinctPercentage.push(a.precinctPercentage), results.precinctsReporting.push(a.precinctsReporting), results.precinctsTotal.push(a.precinctsTotal), void 0) }), g(state), i = 0; i < towns.length; i++) { var j = questionData.towns[towns[i]].candidates.Yes.votePercent - questionData.towns[towns[i]].candidates.No.votePercent; questionData.towns[towns[i]].diff = j, results.diff.push(j); var k; questionData.towns[towns[i]].precinctPercentage < 50 ? k = "nodata" : j > 40 ? k = "y5" : j > 30 ? k = "y4" : j > 20 ? k = "y3" : j > 10 ? k = "y2" : j > 1 ? k = "y1" : j > -1 ? k = "tied" : j > -10 ? k = "n1" : j > -20 ? k = "n2" : j > -30 ? k = "n3" : j > -40 ? k = "n4" : j <= -40 && (k = "n5"), questionData.towns[towns[i]].colorcode = k, results.colorcode.push(k), tablebody[i] = tablebody[i] ? tablebody[i] : [], tablebody[i].push(questionData.towns[towns[i]].townName), parties.forEach(function (a) { tablebody[i].push(d3.format(",")(questionData.towns[towns[i]].candidates[a].votes)), tablebody[i].push(questionData.towns[towns[i]].candidates[a].votePercent + "%") }), tablebody[i].push(questionData.towns[towns[i]].precinctPercentage + "%") } var l; d3.json("http://localhost:54817/ma-towns.topojson.json", function (a, b) { l = new c(b), $(".townshape").on("click focus", function () { f($(this).attr("data-town")) }), $(".townshape").mousemove(function (a) { "none" !== $(".infobox").css("display") && _.debounce(d($(this).attr("data-town"), a.clientX, a.clientY), 300) }), $(".townshape").mouseout(function () { e() }) }), h(results, townbytown), $("#resultstable").DataTable({ paging: !1 }), pymChild.sendHeight(), $("#tooltiptable, .pctreported").hide() }); var pymChild = new pym.Child({ polling: 1e3 }); $("#change-questions").change(function () { window.location = this.value }), $("#refresh").click(function () { window.location.reload() }); var refreshcount = 30, refreshbutton = setInterval(function () { refreshcount--, $("#refresh span").text("in " + refreshcount), refreshcount < 1 && (clearInterval(refreshbutton), $("#refresh span").empty(), $("#refresh").prop("disabled", !1)) }, 1e3);