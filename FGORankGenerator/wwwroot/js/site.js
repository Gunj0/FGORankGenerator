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
// ローカルストレージを全消去
function clearLocalStorage() {
    try {
        localStorage.clear();
        return true;
    }
    catch (e) {
        return "error";
    }
}
// ローカルストレージから所持チェック復元
for (let i = 0, len = localStorage.length; i < len; i++) {
    const key = localStorage.key(i);
    const check = +localStorage.getItem(key);
    if (check == 1) {
        //const keyColumn = document.querySelector(`#${key}`);
        $('#' + key).toggleClass("bg-warning");
    }
}
// テーブルクリック
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
            }
            else {
                localStorage.setItem(id, '1');
            }
        }
    });
});
// テーブルソート(https://qiita.com/fromage-blanc/items/94b90e2b9431884ad6fc)
$(document).ready(function () {
    $('#sort_table').tablesorter();
});
// 全チェック処理
function allCheck() {
    $('tbody tr').each(function (i, elem) {
        let id = $(this).attr("id");
        localStorage.setItem(id, '1');
    });
}
;
// 全チェッククリック
$(document).ready(function () {
    $('#all-check').click(function () {
        var result = window.confirm("全てをチェック状態にしますか？");
        if (result) {
            allCheck();
            window.location.reload();
        }
    });
});
// チェックリセットクリック
$(document).ready(function () {
    $('#check-reset').click(function () {
        var result = window.confirm("チェック状態をリセットしますか？");
        if (result) {
            clearLocalStorage();
            window.location.reload();
        }
    });
});
//# sourceMappingURL=site.js.map