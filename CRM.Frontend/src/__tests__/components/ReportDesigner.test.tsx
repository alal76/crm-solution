import React from 'react';
import { fireEvent, screen } from '@testing-library/react';
import { ReportDesigner } from '../../components/analytics/ReportDesigner';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

describe('ReportDesigner', () => {
  it('shows an error when report name is empty', () => {
    renderWithProviders(<ReportDesigner onSave={jest.fn()} />);

    const nameInput = screen.getByLabelText(/report name/i);
    fireEvent.change(nameInput, { target: { value: '' } });

    fireEvent.click(screen.getByRole('button', { name: /save report/i }));

    expect(screen.getByText(/report name is required/i)).toBeInTheDocument();
  });

  it('shows an error when report name is not unique', () => {
    renderWithProviders(
      <ReportDesigner
        onSave={jest.fn()}
        existingReportNames={['Sales Pipeline']}
      />
    );

    const nameInput = screen.getByLabelText(/report name/i);
    fireEvent.change(nameInput, { target: { value: 'Sales Pipeline' } });

    fireEvent.click(screen.getByText(/opportunity name/i));
    fireEvent.click(screen.getByRole('button', { name: /save report/i }));

    expect(screen.getByText(/report name must be unique/i)).toBeInTheDocument();
  });

  it('shows an error when report query is missing', () => {
    renderWithProviders(<ReportDesigner onSave={jest.fn()} />);

    const nameInput = screen.getByLabelText(/report name/i);
    fireEvent.change(nameInput, { target: { value: 'Unique Report' } });

    fireEvent.click(screen.getByRole('button', { name: /save report/i }));

    expect(screen.getByText(/report query is required/i)).toBeInTheDocument();
  });

  // REV-FE-007: an active filter with an invalid value must block Save/Run,
  // both via the button's disabled state and via the handler's own guard.
  it('blocks Save and Run while an active filter has a validation error, and re-enables once fixed', async () => {
    const onSave = jest.fn().mockResolvedValue(undefined);
    const onRun = jest.fn().mockResolvedValue(undefined);
    renderWithProviders(<ReportDesigner onSave={onSave} onRun={onRun} />);

    // Add a column so Save/Run aren't gated by the "no columns" rule instead.
    fireEvent.click(screen.getByText(/opportunity name/i));

    // Add a filter and point it at a numeric column (Amount).
    fireEvent.click(screen.getByRole('tab', { name: /filters/i }));
    fireEvent.click(screen.getByRole('button', { name: /add filter/i }));

    const comboboxes = screen.getAllByRole('combobox');
    fireEvent.mouseDown(comboboxes[1]); // filter "field" select
    fireEvent.click(screen.getByRole('option', { name: 'Amount' }));

    const saveButton = screen.getByRole('button', { name: /save report/i });
    const runButton = screen.getByRole('button', { name: /run report/i });
    expect(saveButton).not.toBeDisabled();
    expect(runButton).not.toBeDisabled();

    // "1e5" is a value the browser accepts as valid scientific notation for a
    // number input, but validateFilterValue's plain-digit regex rejects it —
    // so the field's own onBlur validation flags it without jsdom's number
    // input sanitization stripping the value out from under the test.
    const numberInput = screen.getByLabelText('Filter numeric value');
    fireEvent.change(numberInput, { target: { value: '1e5' } });
    fireEvent.blur(numberInput);

    expect(screen.getByText(/please enter a numeric value/i)).toBeInTheDocument();
    expect(saveButton).toBeDisabled();
    expect(runButton).toBeDisabled();

    // Buttons are disabled, but also assert the handlers themselves refuse to
    // proceed (guards against a stale/re-enabled button or keyboard submit).
    fireEvent.click(saveButton);
    fireEvent.click(runButton);
    expect(onSave).not.toHaveBeenCalled();
    expect(onRun).not.toHaveBeenCalled();

    // Fix the value — the error clears and both actions re-enable.
    fireEvent.change(numberInput, { target: { value: '500' } });
    fireEvent.blur(numberInput);

    expect(screen.queryByText(/please enter a numeric value/i)).not.toBeInTheDocument();
    expect(saveButton).not.toBeDisabled();
    expect(runButton).not.toBeDisabled();
  });

  // An inactive filter's value is not applied to the report (see the
  // "Filters:" summary count, which is also filtered on isActive), so a
  // leftover invalid value on a disabled filter must not block submission.
  it('does not block Save when the only filter with a validation error is inactive', () => {
    const onSave = jest.fn().mockResolvedValue(undefined);
    renderWithProviders(<ReportDesigner onSave={onSave} />);

    fireEvent.click(screen.getByText(/opportunity name/i));
    fireEvent.click(screen.getByRole('tab', { name: /filters/i }));
    fireEvent.click(screen.getByRole('button', { name: /add filter/i }));

    const comboboxes = screen.getAllByRole('combobox');
    fireEvent.mouseDown(comboboxes[1]);
    fireEvent.click(screen.getByRole('option', { name: 'Amount' }));

    const numberInput = screen.getByLabelText('Filter numeric value');
    fireEvent.change(numberInput, { target: { value: '1e5' } });
    fireEvent.blur(numberInput);

    const saveButton = screen.getByRole('button', { name: /save report/i });
    expect(saveButton).toBeDisabled();

    // Deactivate the filter — its stale error should no longer count.
    fireEvent.click(screen.getByRole('checkbox'));

    expect(saveButton).not.toBeDisabled();
  });
});
