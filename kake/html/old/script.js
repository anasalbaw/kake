var volume = 0;
var errorCount = 0;
function updateSong(title)
{
  var np = $('#nowplaying ul');
  if(title === $('li:first span:first', np).text()) return false;
  var wasAd = ($('li:first span:first', np).text() == "advertisement");
  if(wasAd)
    $('li:first', np)
      .stop()
      .slideUp(700,function (){$(this).remove();});
  else
    $('li:first', np)
      .stop()
      .animate({'color':'#909090'},700);
  
  $('<li />')
    .css({
      'color':'#8DB714',
      'margin-top':'-30px',
      'opacity':'0'
    })
    .stop()
    .animate({
      'marginTop':'0',
      'opacity':1
    }, 700)
    .prependTo(np)
    .append($('<span />').text(title));
  if(!wasAd)
    $('li:gt(2)', np)
      .stop()
      .animate({
        'marginTop':'20px',
        'opacity':0
      }, 1500, function () {
        $(this).remove();
      });  
}

function cmd(c, noEvent)
{
  $.getJSON('ajax/'+c, process).fail(function (){$('#error').text('Error occured while trying to "'+c+'"!');});
  if(noEvent == undefined) event.preventDefault();
}

function updateNowPlaying()
{
  cmd('getNowPlaying', true);
  setTimeout("updateNowPlaying()",5000);
}

function process(data)
{
  if(data.muted==1)
  {
    $('#muteButton').text('unmute');
    $('#muteButton').attr('href','./unmute');
  }
  else
  {
    $('#muteButton').text('mute');
    $('#muteButton').attr('href','./mute');
  }
  if(data.playing == 1)
    $('#btnPlayPause').attr('src','pause.png');
  else
    $('#btnPlayPause').attr('src','play.png');
  if(volume!=data.volume)
  {
    volume=data.volume;
    var w=volume*100;
    $("#volumeBar").stop().animate({'width':w+"px"},400);
    $("#volumeBarBg").stop().animate({'width':(100-w)+"px"},400);
  }
  $("#queueInfo").text(data.queue > 0 ? data.queue+' song'+(data.queue==1 ? '' : 's')+' in queue' : '');
  updateSong(data.songTitle);
  clearError();
}

function toggleQueueURI()
{
  if($('#queueURIcontainer').is(':visible'))
  {
    $('#fader').fadeOut(400);
    $('#queueURIcontainer').fadeOut(400);
  }else{
    $('#fader').fadeIn(400);
    $('#queueURIcontainer').fadeIn(400);
  }
  event.preventDefault();
}

function toggleQueueSearch()
{
  if($('#queueSearchContainer').is(':visible'))
  {
    $('#fader').fadeOut(400);
    $('#queueSearchContainer').fadeOut(400);
    $('#keywords').val('');
  }else{
    $('#fader').fadeIn(400);
    $('#queueSearchContainer').fadeIn(400);
  }
  event.preventDefault();
}

function error(str)
{
  errorCount++;
  if(errorCount > 2) $('#error').text(str);
}

function clearError()
{
  errorCount = 0;
  $('#error').text('');
}

function search(keywords, noEvent)
{
  $.getJSON('ajax/search?q='+keywords, processResults).fail(function (){error('Error occured while searching!');});
  if(noEvent == undefined) event.preventDefault();
}

function processResults(data)
{
  $('#searchError').text((data.error ? data.error : ""));
  $('#searchResults').empty();
  if(data.results!=null)
  {
    for(var i=0;i<data.results.length;i++)
    {
      $('<tr />')
        .append($('<td />')
        .text(data.results[i].name))
        .append($('<td />')
        .text(data.results[i].artist))
        .appendTo('#searchResults')
        .data("uri",data.results[i].uri)
        .click(function(){queueURI($(this).data("uri"));toggleQueueSearch();});
    }
  }
}

function queueURI(uri) {cmd('queue/'+uri);}
