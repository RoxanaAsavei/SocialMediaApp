// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    const description = document.getElementById('userDescription');
    const charCount = document.getElementById('charCount');
    const charCounter = document.getElementById('charCounter');
    const maxLength = 100;

    if (description && charCount && charCounter) {
        description.addEventListener('input', () => {
            const currentLength = description.value.length;
            charCount.textContent = currentLength;

            if (currentLength >= maxLength) {
                charCounter.classList.add('warning');
            } else {
                charCounter.classList.remove('warning');
            }

            if (currentLength > maxLength) {
                description.value = description.value.substring(0, maxLength);
            }
        });
    }

    var profileCards = document.querySelectorAll('.p-card');
    profileCards.forEach(function (card) {
        card.addEventListener('click', function () {
            var userId = this.getAttribute('data-user-id');
            window.location.href = '/Users/Details/' + userId;
        });
    });


    function toggleHeartClass(elementId) {
        var element = document.getElementById(elementId);
        if (element) {
            if (element.classList.contains('bi-heart')) {
                element.classList.remove('bi-heart');
                element.classList.add('bi-heart-fill');
            }

            else if (element.classList.contains('bi-heart-fill')) {
                element.classList.remove('bi-heart-fill');
                element.classList.add('bi-heart');
            }
        }
    }

});
