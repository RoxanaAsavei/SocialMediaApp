// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const description = document.getElementById('userDescription');
const charCount = document.getElementById('charCount');
const charCounter = document.getElementById('charCounter');
const maxLength = 100; 

description.addEventListener('input', () => {
    const currentLength = description.value.length;
    charCount.textContent = currentLength;

    if (currentLength >= maxLength) {
        charCounter.classList.add('warning');
    } else {
        charCounter.classList.remove('warning');
    }

    if (currentLength > maxLength) {
        textarea.value = textarea.value.substring(0, maxLength);
    }
});