<script>
import ReportIcon from '@/assets/Icons/ReportIcon.vue';
import { reportService } from '@/services/report';

export default {
  name: "ReportPage",
  components: {
    ReportIcon
  },
  data() {
    return {
      reportReasons: [],
      activeDropdown: null,
      selectedReason: null,
      reasonDescription: '',
      errors: {
        reason: false,
        description: false
      }
    }
  },
  watch: {
    selectedReason() {
      this.errors.reason = false;
      if (this.selectedReason?.reasonName !== 'Inne') {
        this.errors.description = false;
      }
    },
    reasonDescription() {
      if (this.selectedReason?.reasonName === 'Inne') {
        this.errors.description = false;
      }
    }
  },
  async mounted() {
    this.reportReasons = this.isPostReport
      ? await reportService.getPostReportReasons()
      : await reportService.getUserReportReasons();
  },
  computed: {
    reportedUsername() {
      return this.$route.query.username;
    },
    reportedPostId() {
      return this.$route.query.postId;
    },
    isPostReport() {
      return this.reportedPostId ? true : false;
    },
    selectedReasonText() {
      return this.selectedReason ? this.selectedReason.reasonName : 'Wybierz powód';
    }
  },
  methods: {
    toggleDropdown(id, event) {
      event.stopPropagation();
      this.activeDropdown = this.activeDropdown === id ? null : id;
    },
    handleClickOutside(event) {
      if (this.activeDropdown) {
        const dropdownEl = this.$refs[this.activeDropdown];
        if (dropdownEl && !dropdownEl.contains(event.target)) {
          this.activeDropdown = null;
        }
      }
    },
    validateForm() {
      this.errors.reason = !this.selectedReason;
      this.errors.description = this.selectedReason?.reasonName === 'Inne' && !this.reasonDescription.trim();

      return !this.errors.reason && (!this.errors.description || this.selectedReason?.reasonName !== 'Inne');
    },
    async report() {
      if (!this.validateForm()) return;

      try {
        const formData = new FormData();
        formData.append('reason', this.selectedReason.id);
        if (this.selectedReason.reasonName === 'Inne') {
          formData.append('description', this.reasonDescription);
        }
        if (this.isPostReport) {
          formData.append('postId', this.reportedPostId);
        } else {
          formData.append('username', this.reportedUsername);
        }
        await reportService.reportContent(formData, this.isPostReport ? 'post' : 'user');

        this.$router.push({ name: 'feed' });
      } catch (error) {
        console.error('Failed to create report:', error);
      }
    }
  },
  created() {
    document.addEventListener('click', this.handleClickOutside);
  },
  beforeUnmount() {
    document.removeEventListener('click', this.handleClickOutside);
  }
}
</script>

<template>
  <main>
    <h2 class="page-name">Reklamacja {{ isPostReport ? 'postu' : 'Użytkownika' }}</h2>
    <section class="page background">
      <span class="cell">
        <h3>Wybierz powód:</h3>
        <div class="dropdown-check-list" :class="{ visible: activeDropdown === 'reason', error: errors.reason }"
          ref="reason">
          <span class="anchor" @click="toggleDropdown('reason', $event)">{{ selectedReasonText }}</span>
          <ul class="items">
            <li v-for="reason in reportReasons" :key="reason.id" @click="selectedReason = reason">
              <input type="radio" :id="'reason-' + reason.id" :value="reason" v-model="selectedReason">
              <label :for="'reason-' + reason.id">{{ reason.reasonName }}</label>
            </li>
          </ul>
        </div>
      </span>
      <span class="cell">
        <h3>Opis:</h3>
        <textarea class="reason-description" :class="{ error: errors.description }" rows="5"
          placeholder="Opisz powód zgłoszenia" v-model="reasonDescription"></textarea>
      </span>
      <button class="report-button" @click="report">
        Zgłoś
        <ReportIcon />
      </button>
    </section>
  </main>
</template>

<style scoped>
.page {
  padding: 20px;
  width: max-content;
  min-width: 300px;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.cell {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.report-button {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  padding: 10px;
  border-radius: 10px;
  background: var(--element-light-color);
  border: 0.5px solid var(--element-border-light-color);
  font-size: 14px;
  font-family: var(--font);
  cursor: pointer;
  transition: transform 0.2s;
}

.report-button:hover {
  background: var(--element-hover-light-color);
}

.report-button:active {
  transform: scale(0.95);
  transition: transform 0.2s;
}

.reason-description {
  padding: 8px 15px;
  font-size: 13px;
}

.dropdown-check-list {
  width: 100%;
  position: relative;
  background: var(--element-light-color);
  border-radius: 10px;
  border: 0.5px solid var(--element-border-light-color);
  font-size: 12px;
  font-weight: 700;
}

.dropdown-check-list .anchor {
  position: relative;
  cursor: pointer;
  display: block;
  padding: 8px 15px;
  border-radius: 10px;
  color: var(--placeholder-color);
}

.dropdown-check-list .anchor:after {
  position: absolute;
  content: "";
  border-left: 1px solid var(--placeholder-color);
  border-top: 1px solid var(--placeholder-color);
  padding: 3px;
  right: 10px;
  top: 35%;
  transform: rotate(-135deg);
  transition: transform 0.4s;
}

.dropdown-check-list.visible .anchor:after {
  transform: rotate(45deg);
  top: 40%;
}

.dropdown-check-list ul.items {
  padding: 5px;
  display: none;
  margin: 0;
  border: 1px solid white;
  border-top: none;
  border-radius: 0 0 10px 10px;
  position: absolute;
  width: 100%;
  background: #aaa;
  backdrop-filter: blur(50px);
  box-sizing: border-box;
  z-index: 100;
  max-height: 150px;
  overflow-y: auto;
}

.dropdown-check-list ul.items li {
  list-style: none;
  padding: 10px;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.dropdown-check-list ul.items li input[type="radio"] {
  margin: 0;
}

.dropdown-check-list ul.items li:hover {
  background: var(--element-hover-light-color);
}

.dropdown-check-list ul.items li label {
  cursor: pointer;
}

.dropdown-check-list.visible .items {
  display: block;
}

.error {
  border-color: var(--placeholder-error-color) !important;
  color: var(--placeholder-error-color);
}

.error::placeholder {
  color: var(--placeholder-error-color);
}

.dropdown-check-list.error .anchor {
  color: var(--placeholder-error-color);
}
</style>