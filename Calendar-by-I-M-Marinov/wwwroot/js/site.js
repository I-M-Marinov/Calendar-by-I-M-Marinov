// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Function to toggle date input types

document.addEventListener('DOMContentLoaded', function () {
    var startDateInput = document.getElementById('startDateInput');
    var endDateInput = document.getElementById('endDateInput');
    var startDateLabel = document.getElementById('startDateLabel');
    var endDateLabel = document.getElementById('endDateLabel');
    var isAllDayCheckbox = document.getElementById('isAllDayCheckbox');
    var eventTypeSelect = document.getElementById('eventTypeSelect');

    // Get the initial values of start and end dates 
    var startDateValue = startDateInput.getAttribute('data-value') || startDateInput.value || '';
    var endDateValue = endDateInput.getAttribute('data-value') || endDateInput.value || '';

    function formatDate(dateString) {
        var date = new Date(dateString);
        // Adjust to local time zone
        var localDate = new Date(date.getTime() - (date.getTimezoneOffset() * 60000));
        return localDate.toISOString().slice(0, 16); // yyyy-MM-ddTHH:mm
    }

    function updateInputTypesAndLabels() {
        var isAllDay = isAllDayCheckbox.checked;
        var eventType = eventTypeSelect.value;

        if (isAllDay || eventType === 'allDay') {
            // Handle all-day events
            endDateInput.style.display = 'none';
            endDateLabel.style.display = 'none';
            startDateInput.type = 'date';
            startDateLabel.textContent = "Date";

            // Set input values formatted as "yyyy-MM-dd"
            if (startDateValue) {
                startDateInput.value = new Date(startDateValue).toISOString().slice(0, 10); // yyyy-MM-dd
            }
        } else {
            // Handle timed events
            startDateLabel.textContent = "Start";
            endDateInput.style.display = 'block';
            endDateLabel.style.display = 'block';
            startDateInput.type = 'datetime-local';
            endDateInput.type = 'datetime-local';

            // Set input values formatted as "yyyy-MM-ddTHH:mm"
            if (startDateValue) {
                startDateInput.value = formatDate(startDateValue); // yyyy-MM-ddTHH:mm
            }
            if (endDateValue) {
                endDateInput.value = formatDate(endDateValue); // yyyy-MM-ddTHH:mm
            }
        }
    }

    isAllDayCheckbox.addEventListener('change', updateInputTypesAndLabels);
    eventTypeSelect.addEventListener('change', updateInputTypesAndLabels);

    // Initial update on page load
    updateInputTypesAndLabels();
});





// FUNCTION TO ADD or REMOVE ATTENDANT'S INPUT FIELDS IN THE CreateEvent View


document.addEventListener('DOMContentLoaded', () => {
    const addAttendeeBtn = document.getElementById('addAttendee');
    const attendeesContainer = document.getElementById('attendeesContainer');

    // Make sure no duplicate event listeners are added
    if (addAttendeeBtn && !addAttendeeBtn.dataset.listenerAdded) {
        addAttendeeBtn.dataset.listenerAdded = true; // Flag to prevent duplicate listeners

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
    }

    // Attach remove button functionality for existing rows, ensure no duplicates
    document.querySelectorAll('.btn-remove-attendee').forEach(button => {
        if (!button.dataset.listenerAdded) {
            button.dataset.listenerAdded = true; // Prevent duplicate listeners

            button.addEventListener('click', () => {
                const row = button.parentElement;
                attendeesContainer.removeChild(row);
            });
        }
    });
});