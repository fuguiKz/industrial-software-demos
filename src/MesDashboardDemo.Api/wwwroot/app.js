const state = {
  overview: null,
  trends: [],
  defects: [],
  alerts: [],
  equipment: [],
  dispatch: null,
};

async function load() {
  const [overview, trends, defects, alerts, equipment, dispatch] = await Promise.all([
    fetchJson("/api/overview"),
    fetchJson("/api/trends/hourly"),
    fetchJson("/api/defects/distribution"),
    fetchJson("/api/alerts"),
    fetchJson("/api/equipment"),
    fetchJson("/api/dispatch/recommendation"),
  ]);

  Object.assign(state, { overview, trends, defects, alerts, equipment, dispatch });

  renderHero();
  renderMetrics();
  renderEquipment();
  renderAlerts();
  renderCharts();
}

async function fetchJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`Request failed: ${url}`);
  }
  return response.json();
}

function renderHero() {
  const root = document.querySelector("#hero-panel");
  root.innerHTML = `
    <div>
      <p class="panel__eyebrow">Focus Line</p>
      <h2>${state.overview.lineName}</h2>
    </div>
    <p>${state.overview.focus}</p>
    <p>当前班次：${state.overview.shiftName}</p>
    <p>运行设备：${state.overview.runningEquipment} 台，告警设备：${state.overview.alarmEquipment} 台</p>
    <p>${state.overview.recommendation}</p>
  `;
}

function renderMetrics() {
  const metrics = [
    ["OEE", `${state.overview.oee.toFixed(1)}%`, "综合效率"],
    ["Yield", `${state.overview.yield.toFixed(1)}%`, "一次通过率"],
    ["CT", `${state.overview.cycleTimeSeconds.toFixed(2)} s`, "平均节拍"],
    ["Output", state.overview.output.toLocaleString("en-US"), "班次累计产出"],
  ];

  document.querySelector("#metrics").innerHTML = metrics
    .map(([label, value, meta]) => `
      <article class="metric-card">
        <div class="metric-card__label">${label}</div>
        <div class="metric-card__value">${value}</div>
        <div class="metric-card__meta">${meta}</div>
      </article>
    `)
    .join("");
}

function renderEquipment() {
  document.querySelector("#equipment-list").innerHTML = state.equipment
    .map((item) => `
      <article class="equipment-card">
        <div class="equipment-card__top">
          <div class="equipment-card__title">${item.code} · ${item.model}</div>
          <span class="status-pill" data-state="${item.status}">${item.status}</span>
        </div>
        <div class="equipment-card__meta">
          利用率 ${item.utilization.toFixed(1)}%，队列 ${item.queueCount} 批次<br />
          ${item.note}
        </div>
      </article>
    `)
    .join("");
}

function renderAlerts() {
  document.querySelector("#dispatch-card").innerHTML = `
    <div class="dispatch-card__title">${state.dispatch.decision}</div>
    <div class="dispatch-card__meta">${state.dispatch.reason}</div>
  `;

  document.querySelector("#alert-list").innerHTML = state.alerts
    .map((item) => `
      <article class="alert-card">
        <div class="alert-card__top">
          <div class="alert-card__title">${item.equipmentCode}</div>
          <span class="severity" data-level="${item.severity}">${item.severity}</span>
        </div>
        <div class="alert-card__meta">${item.message}<br />${formatTime(item.occurredAt)} · ${item.code}</div>
      </article>
    `)
    .join("");
}

function renderCharts() {
  if (!window.echarts) {
    return;
  }

  const trendChart = echarts.init(document.querySelector("#trend-chart"));
  const defectChart = echarts.init(document.querySelector("#defect-chart"));

  trendChart.setOption({
    backgroundColor: "transparent",
    tooltip: { trigger: "axis" },
    legend: {
      top: 0,
      textStyle: { color: "#cbd7f5" },
    },
    grid: { left: 36, right: 24, top: 44, bottom: 30 },
    xAxis: {
      type: "category",
      data: state.trends.map((item) => item.hour),
      axisLine: { lineStyle: { color: "rgba(203, 215, 245, 0.25)" } },
      axisLabel: { color: "#9ab0dc" },
    },
    yAxis: [
      {
        type: "value",
        name: "%",
        axisLabel: { color: "#9ab0dc" },
        splitLine: { lineStyle: { color: "rgba(203, 215, 245, 0.08)" } },
      },
      {
        type: "value",
        name: "s",
        axisLabel: { color: "#9ab0dc" },
        splitLine: { show: false },
      },
    ],
    series: [
      {
        name: "OEE",
        type: "line",
        smooth: true,
        data: state.trends.map((item) => item.oee),
        lineStyle: { color: "#5a8cff", width: 3 },
        itemStyle: { color: "#5a8cff" },
      },
      {
        name: "Yield",
        type: "line",
        smooth: true,
        data: state.trends.map((item) => item.yield),
        lineStyle: { color: "#22c3a6", width: 3 },
        itemStyle: { color: "#22c3a6" },
      },
      {
        name: "CT",
        type: "bar",
        yAxisIndex: 1,
        data: state.trends.map((item) => item.cycleTimeSeconds),
        itemStyle: {
          color: "rgba(255, 186, 85, 0.78)",
          borderRadius: [6, 6, 0, 0],
        },
      },
    ],
  });

  defectChart.setOption({
    backgroundColor: "transparent",
    tooltip: { trigger: "item" },
    legend: {
      bottom: 0,
      textStyle: { color: "#cbd7f5" },
    },
    series: [
      {
        type: "pie",
        radius: ["42%", "72%"],
        center: ["50%", "44%"],
        itemStyle: { borderColor: "#0f1830", borderWidth: 5 },
        label: { color: "#e4edff" },
        data: state.defects.map((item, index) => ({
          value: item.count,
          name: item.name,
          itemStyle: { color: ["#5a8cff", "#22c3a6", "#ffba55", "#ff6b7f", "#8f7dff"][index] },
        })),
      },
    ],
  });

  window.addEventListener("resize", () => {
    trendChart.resize();
    defectChart.resize();
  });
}

function formatTime(value) {
  const date = new Date(value);
  return date.toLocaleString("zh-CN", {
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  });
}

load().catch((error) => {
  document.body.innerHTML = `<pre style="padding:24px;color:#fff">${error.message}</pre>`;
});
