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

// Function to hide the End Date Input and Label when ALL DAY Event is selected

document.addEventListener('DOMContentLoaded', function () {
    var eventTypeSelect = document.getElementById('eventTypeSelect');
    var endDateInput = document.getElementById('endDateInput');
    var endDateLabel = endDateInput.previousElementSibling;
    var startDateInput = document.getElementById('startDateInput');
    var startDateLabel = document.getElementById('startDateLabel');

    function updateEndDateVisibility() {
        var eventType = eventTypeSelect.value;

        if (eventType === 'allDay') {
            // Hide the end date input and label
            endDateInput.style.display = 'none';
            endDateLabel.style.display = 'none';
            startDateLabel.textContent = "Date";

            // Set end date to one day after start date if start date is present
            if (startDateInput.value) {
                var startDate = new Date(startDateInput.value);
                var endDate = new Date(startDate);
                endDate.setDate(startDate.getDate() + 1);
                endDateInput.value = endDate.toISOString().slice(0, 16);
            } else {
                endDateInput.value = '';
            }
        } else {
            // Show the end date input and label
            endDateInput.style.display = 'block';
            endDateLabel.style.display = 'block';
            startDateLabel.textContent = "Start";
        }
    }

    // Attach event listener to event type select
    eventTypeSelect.addEventListener('change', updateEndDateVisibility);

    // Initial check on page load
    updateEndDateVisibility();
});
