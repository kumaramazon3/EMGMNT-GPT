// Global chart variable (declare once)
//let line30Chart = null;
let dailySalesChart = null;// Global variable for daily chart
let weeklySalesChart = null;// Global variable for weekly chart
let monthlySalesChart = null;// Global variable for monthly chart


// Run after DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
    loadDailyProductSales();            // daily DB
    loadWeeklyProductSales_FromAPI();  // fetch and draw weekly chart from controller API
    loadMonthlyProductSales_FromAPI();  // new monthly chart
});

let _fax = "Fax";


// Daily Product Sales Data
function loadDailyProductSales() {
    // Fetch controller data
    fetch('/Home/GetDailyProductSales')
        .then(response => response.json())
        .then(data => {
            drawDailySalesChart(data);
        })
        .catch(err => console.error("Error loading data:", err));
}
function drawDailySalesChart(data) {
    if (dailySalesChart !== null) {
        dailySalesChart.destroy();
    }
    const ctx = document.getElementById('DailyProductSales').getContext('2d');

    dailySalesChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [
                {
                    label: 'Fax',
                    data: data.apple,
                    borderColor: 'red',
                    tension: 0.3
                },
                {
                    label: 'Zirconia',
                    data: data.orange,
                    borderColor: 'orange',
                    tension: 0.3
                },
                {
                    label: 'Alumina',
                    data: data.watermelon,
                    borderColor: 'green',
                    tension: 0.3
                }
            ]
        }
    });
}

// -------------------------------------------
// Load weekly chart (hardcoded data)
// -------------------------------------------
function loadWeeklyProductSales_FromAPI() {
    fetch('/Home/GetWeeklyProductSales')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not OK');
            }
            return response.json();
        })
        .then(data => {
            drawWeeklySalesChart(data.labels, data.apple, data.orange, data.watermelon);
        })
        .catch(error => {
            console.error('Error fetching weekly product sales data:', error);
        });
}
// Chart rendering function (make sure this is defined somewhere globally)
function drawWeeklySalesChart(labels, appleData, orangeData, watermelonData) {
    if (weeklySalesChart !== null) {
        weeklySalesChart.destroy();
    }
    const ctx = document.getElementById('weeklyproductsales').getContext('2d');

    weeklySalesChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Fax',
                    data: appleData,
                    borderColor: 'red',
                    borderWidth: 2,
                    tension: 0.3
                },
                {
                    label: 'Zirconia',
                    data: orangeData,
                    borderColor: 'orange',
                    borderWidth: 2,
                    tension: 0.3
                },
                {
                    label: 'Alumina',
                    data: watermelonData,
                    borderColor: 'green',
                    borderWidth: 2,
                    tension: 0.3
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            },
            plugins: {
                legend: { position: 'bottom' }
            }
        }
    });
}

function loadMonthlyProductSales_FromAPI() {
    fetch('/Home/GetMonthlyProductSales')
        .then(response => {
            if (!response.ok) throw new Error('Network response was not OK');
            return response.json();
        })
        .then(data => {
            drawMonthlySalesChart(data.labels, data.apple, data.orange, data.watermelon);
        })
        .catch(error => {
            console.error('Error fetching monthly product sales data:', error);
        });
}
// Chart rendering function
function drawMonthlySalesChart(labels, appleData, orangeData, watermelonData) {
    if (monthlySalesChart !== null) {
        monthlySalesChart.destroy();
    }
    const ctx = document.getElementById('monthlyproductsales').getContext('2d');
    monthlySalesChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                { label: _fax, data: appleData, borderColor: 'red', borderWidth: 2, tension: 0.3 },
                { label: 'Zirconia', data: orangeData, borderColor: 'orange', borderWidth: 2, tension: 0.3 },
                { label: 'Alumina', data: watermelonData, borderColor: 'green', borderWidth: 2, tension: 0.3 }
            ]
        },
        options: {
            maintainAspectRatio: false,
            responsive: true,
            plugins: { legend: { position: 'bottom' } },
            scales: { y: { beginAtZero: true } }
        }
    });
}





















//function loadMonthlyProductSales_Hardcoded() {
//    // X-axis labels: months
//    const labels = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

//    // Hardcoded data for each product
//    const appleData = [120, 135, 150, 145, 160, 155, 170, 165, 180, 175, 190, 200];
//    const orangeData = [90, 100, 95, 110, 105, 120, 115, 130, 125, 140, 135, 150];
//    const watermelonData = [60, 70, 65, 80, 75, 90, 85, 100, 95, 110, 105, 120];

//    drawMonthlySalesChart(labels, appleData, orangeData, watermelonData);
//}
//// Function to draw monthly chart
//function drawMonthlySalesChart(labels, appleData, orangeData, watermelonData) {
//    // Destroy existing chart if exists
//    if (monthlySalesChart !== null) {
//        monthlySalesChart.destroy();
//    }
//    const ctx = document.getElementById('monthlyproductsales').getContext('2d');
//    monthlySalesChart = new Chart(ctx, {
//        type: 'line',
//        data: {
//            labels: labels,
//            datasets: [
//                {
//                    label: 'Fax',
//                    data: appleData,
//                    borderColor: 'red',
//                    borderWidth: 2,
//                    tension: 0.3
//                },
//                {
//                    label: 'Zirconia',
//                    data: orangeData,
//                    borderColor: 'orange',
//                    borderWidth: 2,
//                    tension: 0.3
//                },
//                {
//                    label: 'Alumina',
//                    data: watermelonData,
//                    borderColor: 'green',
//                    borderWidth: 2,
//                    tension: 0.3
//                }
//            ]
//        },
//        options: {
//            maintainAspectRatio: false,
//            responsive: true,
//            plugins: {
//                legend: { position: 'bottom' }
//            },
//            scales: {
//                y: {
//                    beginAtZero: true
//                }
//            }
//        }
//    });
//}

//function loadWeeklyProductSales_Hardcoded() {
//    // Weekly labels (X-axis)
//    const labels = ["Week 1", "Week 2", "Week 3", "Week 4"];
//    // Weekly totals (hard-coded)
//    const appleData = [45, 38, 42, 50];
//    const orangeData = [28, 32, 30, 26];
//    const watermelonData = [18, 22, 20, 19];
//    drawWeeklySalesChart(labels, appleData, orangeData, watermelonData);
//}

//// -------------------------------------------
//// Draw Weekly Chart
//// -------------------------------------------
//function drawWeeklySalesChart(labels, appleData, orangeData, watermelonData) {

//    if (weeklySalesChart !== null) {
//        weeklySalesChart.destroy();
//    }

//    const ctx = document.getElementById('weeklyproductsales').getContext('2d');

//    weeklySalesChart = new Chart(ctx, {
//        type: 'line',
//        data: {
//            labels: labels,
//            datasets: [
//                {
//                    label: 'Fax',
//                    data: appleData,
//                    borderColor: 'red',
//                    borderWidth: 2,
//                    tension: 0.3
//                },
//                {
//                    label: 'Zirconia',
//                    data: orangeData,
//                    borderColor: 'orange',
//                    borderWidth: 2,
//                    tension: 0.3
//                },
//                {
//                    label: 'Alumina',
//                    data: watermelonData,
//                    borderColor: 'green',
//                    borderWidth: 2,
//                    tension: 0.3
//                }
//            ]
//        },
//        options: {
//            maintainAspectRatio: false,   // fix squeezed chart
//            responsive: true,
//            scales: {
//                y: {
//                    beginAtZero: true     // better scaling
//                }
//            },
//            plugins: {
//                legend: { position: 'bottom' }
//            }
//        }
//    });
//}

//------------------------------------------------------------------------



//function drawProductChart_JShardCodeData() {

//    // Destroy chart if it already exists
//    if (line30Chart !== null) {
//        line30Chart.destroy();
//    }

//    // Generate last 30-days labels
//    const labels = [];
//    for (let i = 29; i >= 0; i--) {
//        const date = new Date();
//        date.setDate(date.getDate() - i);

//        const month = date.toLocaleString('en-US', { month: 'short' });
//        const day = String(date.getDate()).padStart(2, '0');

//        labels.push(`${month}-${day}`);
//    }

//    // Chart datasets (replace with real DB)
//    const appleData = [5, 8, 6, 4, 7, 8, 9, 6, 5, 6, 8, 9, 7, 5, 4, 6, 7, 8, 9, 10, 9, 8, 7, 5, 6, 7, 8, 9, 8, 7];
//    const orangeData = [3, 4, 3, 4, 5, 4, 5, 4, 4, 5, 6, 5, 4, 3, 4, 5, 6, 7, 7, 6, 5, 4, 4, 4, 5, 5, 6, 5, 4, 3];
//    const watermelonData = [2, 3, 2, 3, 4, 3, 3, 3, 4, 4, 5, 4, 3, 3, 3, 4, 5, 6, 6, 5, 4, 3, 3, 3, 3, 4, 4, 3, 2, 2];

//    const ctx = document.getElementById('line30_productChart').getContext('2d');

//    // Create new chart and store reference
//    line30Chart = new Chart(ctx, {
//        type: 'line',
//        data: {
//            labels: labels,
//            datasets: [
//                {
//                    label: 'Apple',
//                    data: appleData,
//                    borderColor: 'red',
//                    tension: 0.3,
//                },
//                {
//                    label: 'Zirconia',
//                    data: orangeData,
//                    borderColor: 'orange',
//                    tension: 0.3,
//                },
//                {
//                    label: 'Alumina',
//                    data: watermelonData,
//                    borderColor: 'green',
//                    tension: 0.3,
//                }
//            ]
//        }
//    });
//}

