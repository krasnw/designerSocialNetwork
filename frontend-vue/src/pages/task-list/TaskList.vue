<script>
import TaskView from './TaskView.vue';

export default {
  name: "TaskListPage",
  components: {
    TaskView
  },
  data() {
    return {
      tasks: [
        {
          username: 'User 1',
          userProfilePicture: 'https://placehold.co/100x100',
          description: 'Task 1 description lorem ipsum dolor sit amet.'
        },
        {
          username: 'User 2',
          userProfilePicture: 'https://placehold.co/100x100',
          description: 'Task 2 description lorem ipsum dolor sit amet.'
        },
        {
          username: 'User 3',
          userProfilePicture: 'https://placehold.co/100x100',
          description: 'Task 3 description lorem ipsum dolor sit amet.'
        }
      ]
    }
  }
};
</script>

<template>
  <main>
    <h2 class="page-name">Task list page</h2>
    <section class="task-list">
      <article v-for="(task, index) in tasks" :key="index">
        <TaskView :username="task.username" :description="task.description"
          :userProfilePicture="task.userProfilePicture" />
      </article>
    </section>
  </main>
</template>

<style scoped>
.task-list {
  display: flex;
  flex-direction: column;
  gap: 50px;
  padding-bottom: 60px;
}
</style>