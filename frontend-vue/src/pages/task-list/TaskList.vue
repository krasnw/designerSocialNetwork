<script>
import Spinner from '@/assets/Icons/Spinner.vue';
import TaskView from './TaskView.vue';
import { taskService } from '@/services/task';

export default {
  name: "TaskListPage",
  components: {
    TaskView,
    Spinner
  },
  methods: {
    handleTaskDelete(taskId) {
      this.tasks = this.tasks.filter(task => task.id !== taskId);
    }
  },
  data() {
    return {
      isLoading: true,
      tasks: []
    }
  },
  async created() {
    this.tasks = await taskService.getTasks().catch(() => {
      return [];
    });
    this.isLoading = false;
  }
};
</script>

<template>
  <main>
    <h2 class="page-name">Task page</h2>
    <section>
      <div v-if="isLoading" class="loading">Loading
        <Spinner class="spinner" />
      </div>
      <section class="task-list" v-else>
        <article v-for="task in tasks" :key="task.id">
          <TaskView :task="task" @task-deleted="handleTaskDelete" />
        </article>
        <div class="no-tasks" v-if="!tasks.length">
          No tasks found
        </div>
      </section>
    </section>
  </main>
</template>

<style scoped>
.task-list {
  display: flex;
  flex-direction: column;
  gap: 30px;
  padding-bottom: 60px;
}

.loading {
  font-size: 14px;
  font-weight: 600;
  color: var(--info-text-color);
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-light-color);
  padding: 20px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  display: flex;
  gap: 15px;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  width: max-content;
}

.no-tasks {
  font-size: 20px;
  font-weight: 600;
  color: var(--info-text-color);
  background-color: var(--element-dark-color);
  border: 0.5px solid var(--element-light-color);
  padding: 40px 80px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  width: max-content;
}
</style>