import { mount } from "@vue/test-utils";
import Settings from "@/pages/settings/Settings.vue";
import SideBar from "@/components/Sidebar/SideBar.vue";

describe("Sidebar Button Visibility Tests", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("should hide sidebar button when checkbox is checked", async () => {
    const settingsWrapper = mount(Settings);

    const checkbox = settingsWrapper.find('input[type="checkbox"]');
    await checkbox.setChecked(true);
    await checkbox.trigger("change");

    expect(localStorage.getItem("sidebarAlwaysOpen")).toBe("true");

    const sidebarWrapper = mount(SideBar, {
      props: { page: "feed" },
    });

    expect(sidebarWrapper.find(".arrow-button").exists()).toBe(false);
  });

  it("should show sidebar button when checkbox is unchecked", async () => {
    const settingsWrapper = mount(Settings);

    const checkbox = settingsWrapper.find('input[type="checkbox"]');
    await checkbox.setChecked(false);
    await checkbox.trigger("change");

    expect(localStorage.getItem("sidebarAlwaysOpen")).toBe("false");

    const sidebarWrapper = mount(SideBar, {
      props: { page: "feed" },
    });

    expect(sidebarWrapper.find(".arrow-button").exists()).toBe(true);
  });
});
