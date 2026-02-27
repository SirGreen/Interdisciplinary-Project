// Fetch motor counts by type
async function fetchMotorCounts() {
    try {
        const response = await fetch('/api/motors/count-by-type');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        const types = Object.keys(data);
        const counts = Object.values(data);
        const totalCount = counts.reduce((total, count) => total + count, 0);

        // Calculate percentages for each motor type
        const percentages = counts.map(count => ((count / totalCount) * 100));
        
        renderMotorTypeChart(types, percentages);
        
        return data;
    } catch (error) {
        console.error("Error fetching motor counts:", error);
        return {};
    }
}
function renderMotorTypeChart(types, counts) {
    let optionsVisitorsProfile = {
      series: counts,
      labels: types,
      colors: ["#435ebe", "#55c6e8"],
      chart: {
        type: "donut",
        width: "100%",
        height: "350px",
      },
      legend: {
        position: "bottom",
      },
      plotOptions: {
        pie: {
          donut: {
            size: "30%",
          },
        },
      },
    }
  let chartVisitorsProfile = new ApexCharts(
    document.getElementById("chart-visitors-profile"),
    optionsVisitorsProfile
  )
  chartVisitorsProfile.render()
}
document.addEventListener('DOMContentLoaded', fetchMotorCounts);

async function fetchMotorEffi() {
    try {
        const response = await fetch('/api/motors/filter-catalogs-by-efficiency');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        const names = data.map(ele => ele.range);
        const counts = data.map(ele => ele.count);
        renderMotorEffiChart(names, counts);
        
        return data;
    } catch (error) {
        console.error("Error fetching motor counts:", error);
        return {};
    }
}
function renderMotorEffiChart(types, counts) {
    let optionsProfileVisit = {
      annotations: {
        position: "back",
      },
      dataLabels: {
        enabled: false,
      },
      chart: {
        type: "bar",
        height: 300,
      },
      fill: {
        opacity: 1,
      },
      plotOptions: {},
      series: [
        {
          name: "amount",
          data: counts,
        },
      ],
      colors: "#435ebe",
      xaxis: {
        categories: types,
      },
    }
  let chartProfileVisit = new ApexCharts(
    document.querySelector("#chart-profile-visit"),
    optionsProfileVisit
  )
  chartProfileVisit.render()
}
document.addEventListener('DOMContentLoaded', fetchMotorEffi);

async function fetchMotorHP() {
    try {
        const response = await fetch('/api/motors/filter-catalogs-by-output-hp');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        const names = data.map(ele => ele.range);
        const counts = data.map(ele => ele.count);
        renderMotorHPChart(names, counts);
        
        return data;
    } catch (error) {
        console.error("Error fetching motor counts:", error);
        return {};
    }
}
function renderMotorHPChart(types, counts) {
    let optionsProfileVisit = {
      annotations: {
        position: "back",
      },
      dataLabels: {
        enabled: false,
      },
      chart: {
        type: "bar",
        height: 300,
      },
      fill: {
        opacity: 1,
      },
      plotOptions: {},
      series: [
        {
          name: "amount",
          data: counts,
        },
      ],
      colors: "#435ebe",
      xaxis: {
        categories: types,
      },
    }
  let chartProfileVisit = new ApexCharts(
    document.querySelector("#chart-profile-visit1"),
    optionsProfileVisit
  )
  chartProfileVisit.render()
}
document.addEventListener('DOMContentLoaded', fetchMotorHP);

async function fetchMotorRPM() {
    try {
        const response = await fetch('/api/motors/filter-catalogs-by-full-load-rpm');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        const names = data.map(ele => ele.range);
        const counts = data.map(ele => ele.count);
        renderMotorRPMChart(names, counts);
        
        return data;
    } catch (error) {
        console.error("Error fetching motor counts:", error);
        return {};
    }
}
function renderMotorRPMChart(types, counts) {
    let optionsProfileVisit = {
      annotations: {
        position: "back",
      },
      dataLabels: {
        enabled: false,
      },
      chart: {
        type: "bar",
        height: 300,
      },
      fill: {
        opacity: 1,
      },
      plotOptions: {},
      series: [
        {
          name: "amount",
          data: counts,
        },
      ],
      colors: "#435ebe",
      xaxis: {
        categories: types,
      },
    }
  let chartProfileVisit = new ApexCharts(
    document.querySelector("#chart-profile-visit2"),
    optionsProfileVisit
  )
  chartProfileVisit.render()
}
document.addEventListener('DOMContentLoaded', fetchMotorRPM);