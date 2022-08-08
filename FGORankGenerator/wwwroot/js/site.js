'use strict';

// ローカルストレージからキーの値を取得
function getLocalStorage(key) {
  try {
    return localStorage.getItem(key);
  }
  catch (e) {
    return "error";
  }
}

// ローカルストレージにキーと値をセット
function setLocalStorage(key, value) {
  try {
    localStorage.setItem(key, value);
    return true;
  }
  catch (e) {
    return "error";
  }
}

// ローカルストレージからチェック状態を復元
for (let i = 1; i < 500; i++) {
  let check = getLocalStorage(i);
  if (check == 1) {
    $('#' + i).toggleClass("bg-warning");
  }
}

// クリック時動作
$(document).ready(function () {
  $('.data').click(function () {
    // 黄色に変更
    $(this).toggleClass("bg-warning");
    // ローカルストレージにチェック保存
    let id = $(this).attr("id");
    let check = getLocalStorage(id);
    if (check != "error") {
      if (check == "1") {
        localStorage.setItem(id, '0');
      } else {
        localStorage.setItem(id, '1');
      }
    }
  });
});