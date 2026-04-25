const queryInput = document.querySelector("#query");
const searchForm = document.querySelector("#search-form");

searchForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  await searchAndRender(queryInput.value.trim());
});

document.querySelectorAll("[data-sample]").forEach((button) => {
  button.addEventListener("click", async () => {
    queryInput.value = button.dataset.sample;
    await searchAndRender(button.dataset.sample);
  });
});

async function boot() {
  const summary = await fetchJson("/api/summary");
  renderSummary(summary);
  await searchAndRender("PKG-240426-00017");
}

async function searchAndRender(query) {
  const units = await fetchJson(`/api/units?query=${encodeURIComponent(query)}`);
  const first = units[0];
  if (!first) {
    renderEmptyState(query);
    return;
  }

  const detail = await fetchJson(`/api/units/${encodeURIComponent(first.serialNumber)}`);
  renderUnit(detail);
}

async function fetchJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`Request failed: ${url}`);
  }
  return response.json();
}

function renderSummary(summary) {
  const items = [
    ["回溯对象", summary.unitCount, "当前示例库中的样本数量"],
    ["异常对象", summary.abnormalCount, "需要工程确认或持续监控"],
    ["最新批次", summary.latestLot, "最近一次更新的 lot"],
  ];

  document.querySelector("#summary-grid").innerHTML = items
    .map(([label, value, meta]) => `
      <article class="summary-card">
        <div class="summary-card__label">${label}</div>
        <div class="summary-card__value">${value}</div>
        <div class="timeline__meta">${meta}</div>
      </article>
    `)
    .join("");
}

function renderUnit(unit) {
  document.querySelector("#unit-detail").innerHTML = `
    <article class="unit-card">
      <div class="unit-card__title">
        <span>${unit.serialNumber}</span>
        <span class="pill" data-grade="${unit.grade}">${unit.grade}</span>
      </div>
      <div class="unit-card__meta">
        批次：${unit.lotNumber}<br />
        封装：${unit.packageName}<br />
        当前站点：${unit.currentStation}<br />
        最近更新时间：${formatTime(unit.lastUpdated)}
      </div>
    </article>
  `;

  document.querySelector("#route-timeline").innerHTML = unit.route
    .map((step) => `
      <article class="timeline-item">
        <div class="timeline__title">${step.stationName} · ${step.status}</div>
        <div class="timeline__meta">${step.equipmentCode}<br />${formatTime(step.timestamp)}</div>
      </article>
    `)
    .join("");

  document.querySelector("#ai-summary").innerHTML = `
    <article class="ai-summary">
      <div class="unit-card__title">
        <span>${unit.aiReview.finalDecision}</span>
        <span class="pill" data-grade="${unit.aiReview.finalDecision}">${unit.aiReview.modelVersion}</span>
      </div>
      <div class="ai-summary__meta">
        结论：${unit.aiReview.finding}<br />
        建议：${unit.aiReview.recommendation}
      </div>
    </article>
  `;

  document.querySelector("#review-grid").innerHTML = unit.imageReviews
    .map((review) => `
      <article class="review-card">
        <div class="review-card__visual">${review.title}</div>
        <div class="review-card__body">
          <div class="review-card__title">${review.source}</div>
          <div class="review-card__meta">
            ${review.summary}<br />
            Verdict: ${review.verdict} · Confidence: ${(review.confidence * 100).toFixed(0)}%
          </div>
        </div>
      </article>
    `)
    .join("");
}

function renderEmptyState(query) {
  document.querySelector("#unit-detail").innerHTML = `
    <article class="unit-card">
      <div class="unit-card__title">未找到对象</div>
      <div class="unit-card__meta">没有匹配 “${query}” 的序列号或批次号。</div>
    </article>
  `;
  document.querySelector("#route-timeline").innerHTML = "";
  document.querySelector("#ai-summary").innerHTML = "";
  document.querySelector("#review-grid").innerHTML = "";
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

boot().catch((error) => {
  document.body.innerHTML = `<pre style="padding:24px">${error.message}</pre>`;
});
