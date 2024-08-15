// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Function to toggle date input types

async function toggleDateTimeInputs() {
    var eventType = document.getElementById("eventTypeSelect").value;
    var startDateInput = document.getElementById("startDateInput");
    var endDateInput = document.getElementById("endDateInput");

    if (eventType === "allDay") {
        // Simulate an asynchronous operation (e.g., fetching data)
        await new Promise(resolve => setTimeout(resolve, 100)); // Example delay

        startDateInput.type = "date"; // Show only date picker
        endDateInput.type = "date";   // Show only date picker
    } else {
        // Simulate an asynchronous operation (e.g., fetching data)
        await new Promise(resolve => setTimeout(resolve, 100)); // Example delay

        startDateInput.type = "datetime-local"; // Show date and time picker
        endDateInput.type = "datetime-local";   // Show date and time picker
    }
}

// Initial check on page load
document.addEventListener("DOMContentLoaded", async function () {
    await toggleDateTimeInputs();
});

// Attach event listener to the event type dropdown
document.getElementById("eventTypeSelect").addEventListener("change", async function () {
    await toggleDateTimeInputs();
});
