
var Interop = {
    setImageRawData: function (dataPtr, width, height) {
        var img = document.createElement("img");
        var parent = document.getElementById('output');
        parent.insertBefore(img, parent.lastChild);

        var rawCanvas = document.getElementById("canvas");
        rawCanvas.width = width;
        rawCanvas.height = height;

        var ctx = rawCanvas.getContext("2d");
        var imgData = ctx.createImageData(width, height);

        var bufferSize = width * height * 4;

        for (var i = 0; i < bufferSize; i += 4) {
            imgData.data[i + 0] = Module.HEAPU8[dataPtr + i + 2];
            imgData.data[i + 1] = Module.HEAPU8[dataPtr + i + 1];
            imgData.data[i + 2] = Module.HEAPU8[dataPtr + i + 0];
            imgData.data[i + 3] = Module.HEAPU8[dataPtr + i + 3];
        }

        ctx.putImageData(imgData, 0, 0);

        img.src = rawCanvas.toDataURL();
    },

    appendResult: function (str) {
        var img = document.createTextNode(str);
        var parent = document.getElementById('results');
        parent.appendChild(img, parent.lastChild);
    }
};

function readTextFile(file) {
    var rawFile = new XMLHttpRequest();
    rawFile.open("GET", file, false);
    rawFile.onreadystatechange = function () {
        if (rawFile.readyState === 4) {
            if (rawFile.status === 200 || rawFile.status == 0) {
                var allLines = rawFile.responseText.split("\n");
                var arrayOfNumbers = allLines.map(Number);
                Interop.setImageRawData(arrayOfNumbers, 15, 10);
            }
        }
    }
    rawFile.send(null);
}

	  Module['onExit'] = function(status) {
		  console.log('onExit');
		  console.log(status);
		  Interop.setImageRawData(status, 1280, 720)
	  }
