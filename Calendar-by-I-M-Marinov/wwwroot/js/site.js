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
    var startDateInput = document.getElementById('startDateInput');
    var endDateInput = document.getElementById('endDateInput');
    var startDateLabel = document.getElementById('startDateLabel');
    var endDateLabel = document.getElementById('endDateLabel');

    function updateEndDateVisibility() {
        var eventType = eventTypeSelect.value;
        var isAllDay = eventType === 'allDay';

        if (isAllDay) {
            // Handle all-day events
            endDateInput.style.display = 'none';
            endDateLabel.style.display = 'none';
            startDateInput.type = 'date';
            endDateInput.type = 'date';
            startDateLabel.textContent = "Date";

            // Convert datetime to date format
            if (startDateInput.value) {
                var startDate = new Date(startDateInput.value);
                startDateInput.value = startDate.toISOString().slice(0, 10); // yyyy-MM-dd
            }
            if (endDateInput.value) {
                var endDate = new Date(endDateInput.value);
                endDateInput.value = endDate.toISOString().slice(0, 10); // yyyy-MM-dd
            }

        } else {
            //  Timed events

            startDateLabel.textContent = "Start";
            endDateInput.style.display = 'block';
            endDateLabel.style.display = 'block';
        }
    }

    eventTypeSelect.addEventListener('change', updateEndDateVisibility);

    updateEndDateVisibility();
});





// FUNCTION TO ADD or REMOVE ATTENDANT'S INPUT FIELDS IN THE CreateEvent View


document.addEventListener('DOMContentLoaded', () => {
    const addAttendeeBtn = document.getElementById('addAttendee');
    const attendeesContainer = document.getElementById('attendeesContainer');

    addAttendeeBtn.addEventListener('click', () => {
        const newAttendeeRow = document.createElement('div');
        newAttendeeRow.classList.add('attendee-row');

        newAttendeeRow.innerHTML = `
            <input type="email" name="Attendants" class="form-control" placeholder="Enter email address" />
            <button type="button" class="btn btn-remove-attendee">Remove</button>
        `;

        attendeesContainer.appendChild(newAttendeeRow);

        // Attach remove button functionality
        newAttendeeRow.querySelector('.btn-remove-attendee').addEventListener('click', () => {
            attendeesContainer.removeChild(newAttendeeRow);
        });
    });

    // Attach remove button functionality for existing rows
    document.querySelectorAll('.btn-remove-attendee').forEach(button => {
        button.addEventListener('click', () => {
            const row = button.parentElement;
            attendeesContainer.removeChild(row);
        });
    });
});

