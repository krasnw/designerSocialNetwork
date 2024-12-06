import { mount } from "@vue/test-utils";
import Settings from "@/pages/settings/Settings.vue";
import SideBar from "@/components/Sidebar/SideBar.vue";

describe("Sidebar Button Visibility Tests", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("should hide sidebar button when checkbox is checked", async () => {
    // Монтируем компоненты
    const settingsWrapper = mount(Settings);

    // Находим чекбокс и кликаем по нему
    const checkbox = settingsWrapper.find('input[type="checkbox"]');
    await checkbox.setChecked(true);
    await checkbox.trigger("change");

    // Проверяем что значение в localStorage установлено
    expect(localStorage.getItem("sidebarAlwaysOpen")).toBe("true");

    // Монтируем сайдбар и проверяем видимость кнопки
    const sidebarWrapper = mount(SideBar, {
      props: { page: "feed" },
    });

    expect(sidebarWrapper.find(".arrow-button").exists()).toBe(false);
  });

  it("should show sidebar button when checkbox is unchecked", async () => {
    // Монтируем компоненты
    const settingsWrapper = mount(Settings);

    // Находим чекбокс и кликаем по нему
    const checkbox = settingsWrapper.find('input[type="checkbox"]');
    await checkbox.setChecked(false);
    await checkbox.trigger("change");

    // Проверяем что значение в localStorage установлено
    expect(localStorage.getItem("sidebarAlwaysOpen")).toBe("false");

    // Монтируем сайдбар и проверяем видимость кнопки
    const sidebarWrapper = mount(SideBar, {
      props: { page: "feed" },
    });

    expect(sidebarWrapper.find(".arrow-button").exists()).toBe(true);
  });
});
