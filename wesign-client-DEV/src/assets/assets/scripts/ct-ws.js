//utilities//

// alert timeout
function alertTimeout() {
  setTimeout(alertHide, 5000);
}

function alertHide() {
  document.querySelector("#alertTimeout").classList.remove("ct-animate-slide-down");
  document.querySelector("#alertTimeout").classList.add("ct-animate-slide-up");
}

// custom select //
function customSelect() {
  var x, i, j, selElmnt, a, b, c;
  /*look for any elements with the class "custom-select":*/
  x = document.getElementsByClassName("select--custom");
  for (i = 0; i < x.length; i++) {
    selElmnt = x[i].getElementsByTagName("select")[0];
    /*for each element, create a new DIV that will act as the selected item:*/
    a = document.createElement("DIV");
    a.setAttribute("class", "select-custom--selected");
    a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;
    x[i].appendChild(a);
    /*for each element, create a new DIV that will contain the option list:*/
    b = document.createElement("DIV");
    b.setAttribute("class", "select-custom__items select-custom-hide");
    for (j = 1; j < selElmnt.length; j++) {
      /*for each option in the original select element,
      create a new DIV that will act as an option item:*/
      c = document.createElement("DIV");
      c.innerHTML = selElmnt.options[j].innerHTML;
      c.addEventListener("click", function (e) {
        /*when an item is clicked, update the original select box,
        and the selected item:*/
        var y, i, k, s, h;
        s = this.parentNode.parentNode.getElementsByTagName("select")[0];
        h = this.parentNode.previousSibling;
        for (i = 0; i < s.length; i++) {
          if (s.options[i].innerHTML == this.innerHTML) {
            s.selectedIndex = i;
            h.innerHTML = this.innerHTML;
            y = this.parentNode.getElementsByClassName("same-as-selected");
            for (k = 0; k < y.length; k++) {
              y[k].removeAttribute("class");
            }
            this.setAttribute("class", "same-as-selected");
            break;
          }
        }
        h.click();
      });
      b.appendChild(c);
    }
    x[i].appendChild(b);
    a.addEventListener("click", function (e) {
      /*when the select box is clicked, close any other select boxes,
      and open/close the current select box:*/
      e.stopPropagation();
      closeAllSelect(this);
      this.nextSibling.classList.toggle("select-custom-hide");
      this.classList.toggle("select-custom__arrow-active");
    });
  }

  // close all checkbox
  function closeAllSelect(elmnt) {
    /*a function that will close all select boxes in the document,
    except the current select box:*/
    var x, y, i, arrNo = [];
    x = document.getElementsByClassName("select-custom__items");
    y = document.getElementsByClassName("select-custom--selected");
    for (i = 0; i < y.length; i++) {
      if (elmnt == y[i]) {
        arrNo.push(i)
      } else {
        y[i].classList.remove("select-custom__arrow-active");
      }
    }
    for (i = 0; i < x.length; i++) {
      if (arrNo.indexOf(i)) {
        x[i].classList.add("select-custom-hide");
      }
    }
  }
  /*if the user clicks anywhere outside the select box,
  then close all select boxes:*/
  document.addEventListener("click", closeAllSelect);
}

// file upload
function fileUpload() {
  document.querySelector("#minput").onchange = function () {
    document.querySelector("#mtxt").innerHTML = this.files.length + " file(s) selected";
  }
}

// input font size
function inputFontSize() {
  document.getElementById("js-input-font-size").onkeyup = function () {
    var value1 = document.getElementById("js-input-font-size").value;
    var value2 = document.getElementById("js-input-font-size").offsetWidth;

   // console.log(value1.length);
   // console.log(value2);

    if (value1.length * 10 > value2) {
      document.getElementById("js-input-font-size").style['font-size'] = "11px";
    } else {
      document.getElementById("js-input-font-size").style['font-size'] = "15px";
    }
  }
}